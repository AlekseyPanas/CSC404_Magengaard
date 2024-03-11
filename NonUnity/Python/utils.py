import os
import pygame
import math
import random
import matplotlib.pyplot as plt
from dataclasses import dataclass
from contextlib import contextmanager
import timeit
import copy
import numpy as np
import pickle
from typing import Callable
from abc import abstractmethod
from gest_base import *
import seaborn
from point_pre_processors import InvertPointsPreProcessor


def run_multi_matching_demo(filenames: list[str], segmentator: Segmentator, err_func: ErrorFunction, points_pre_processor: PointPreProcessor):
    """
    Run a demo where user draws gestures and it displays the best match accuracy among provided gesture files.
    It also displays the segmentation of the user-drawn gesture with colors
    :param filenames: list of file names without ".gest" suffix
    """

    gests: list[list[GestComp]] = []
    for f in filenames:
        with open(f + ".gest", "rb") as file:
            gests.append(pickle.load(file))

    running = True
    screen = pygame.display.set_mode((500, 500))
    clock = pygame.time.Clock()
    track = False
    user_sample = []
    surfs = [visualize_gesture(g) for g in gests]
    matched_gest: int | None = None
    segmentation: set[int] | None = None

    while running:
        screen.fill((255, 255, 255))

        for e in pygame.event.get():
            if e.type == pygame.QUIT:
                running = False
            elif e.type == pygame.MOUSEBUTTONDOWN:
                user_sample = []
                track = True
            elif e.type == pygame.MOUSEBUTTONUP:
                track = False

                if len(set(user_sample)) > 5:
                    errors = []
                    segmentations = []
                    total_time = 0
                    user_sample = points_pre_processor.pre_process_points(user_sample)
                    inverted_sample = InvertPointsPreProcessor().pre_process_points(user_sample)
                    for g in gests:
                        out = []
                        runtime = timeit.timeit("out.append(run_algorithm(inverted_sample, g, segmentator, err_func, points_pre_processor))",
                                                globals=locals() | globals(), number=1)
                        errors.append(out[0][0])
                        segmentations.append(out[0][1])
                        print(f"Runtime: {runtime}")
                        total_time += runtime
                    best_match_idx = min(enumerate(errors), key=lambda x: x[1])[0]
                    matched_gest = best_match_idx
                    segmentation = segmentations[best_match_idx]
                    print(f"Matched Gesture '{filenames[best_match_idx]}'  -----  Error {errors[best_match_idx]}  ----- Total Runtime {total_time}")

        if matched_gest is not None:
            screen.blit(surfs[matched_gest], surfs[matched_gest].get_rect(center=(250, 250)))

        if track:
            user_sample.append(tuple(pygame.mouse.get_pos()))

        if segmentation is not None:
            sorted_seg = sorted(list(segmentation))
            colors = [tuple(int(c*255) for c in col) for col in seaborn.color_palette(None, len(segmentation) + 1)]

        for p in range(len(user_sample)):
            if not track:
                col_idx = len([i for i in sorted_seg if p > i])
                col = colors[col_idx]
            else:
                col = (255, 0, 0)


            pygame.draw.circle(screen, col, user_sample[p], 4)

        pygame.display.update()

        clock.tick(60)


def draw_user_points(pts):
    """
    Display a list of points in a window
    """
    with draw_pygame() as screen:
        for p in pts:
            pygame.draw.circle(screen, (255, 0, 0), (p[0], -p[1]), 3)


def get_user_drawn_points() -> list[tuple[int, int]]:
    """
    Lets user draw points in a realtime pygame window until they release mouse. Return points
    """
    pts = []
    track = False
    def loop(events: list[[pygame.event.Event]], screen) -> bool:
        nonlocal track
        stop = False
        for e in events:
            if e.type == pygame.MOUSEBUTTONDOWN:
                track = True
            elif e.type == pygame.MOUSEBUTTONUP:
                stop = True
            elif e.type == pygame.QUIT:
                stop = True
        if track:
            pts.append(tuple(pygame.mouse.get_pos()))
        for p in pts:
            pygame.draw.circle(screen, (255, 0, 0), p, 4)
        return not stop
    draw_pygame_realtime(loop)

    return pts


@contextmanager
def draw_pygame(size=(500, 500)):
    """
    Use "with" statement to get a pygame screen. Draw a static single-frame output to show
    to the user
    """
    pygame.init()
    screen = pygame.display.set_mode(size)
    screen.fill((255, 255, 255))
    yield screen
    pygame.display.update()
    pygame.event.set_blocked(None)
    pygame.event.set_allowed((pygame.QUIT, pygame.KEYDOWN))
    pygame.event.clear()
    pygame.event.wait()
    pygame.event.set_allowed(None)
    pygame.quit()


def draw_pygame_realtime(loop_body: Callable[[list[pygame.event.Event], pygame.Surface], bool], size=(500, 500)):
    """
    Provides surrounding pygame boilerplate and allows passing in a function to define loop body to get realtime
    user input

    :param loop_body: body of the pygame draw loop providing the surface and events for that frame, returning true
    if the program should end
    """
    pygame.init()
    screen = pygame.display.set_mode(size)
    clock = pygame.time.Clock()

    while True:
        screen.fill((255, 255, 255))
        events = pygame.event.get()
        for e in events:
            if e.type == pygame.QUIT:
                pass
        if not loop_body(events, screen):
            break

        pygame.display.update()
        clock.tick(30)

    pygame.quit()


def slice_dist(pts: list[tuple[float, float]], b: int, e: int):
    """Compute total length of a subsection of a sequence pts
    :param b: lower bound 1 <= b
    :param e: upper bound < e
    """
    length = 0
    for i in range(b, e):
        length += math.dist(pts[i], pts[i - 1])
    return length


def visualize_gesture(gs: list[GestComp]) -> pygame.Surface:
    """Return a surface visualizing a list of gesture components (a.k.a a gesture)"""
    box_size = 450

    surf = pygame.Surface((box_size, box_size), pygame.SRCALPHA, 32)

    # Compute size
    ratio_total = sum([g.relative_ratio for g in gs])
    bounds = [[0, 0], [0, 0]]  # (minx, maxx, miny, maxy)
    cur_pos = [0, 0]
    cur_ang = 0

    def update_bounds():
        nonlocal bounds
        for i in range(2):
            if cur_pos[i] < bounds[i][0]:
                bounds[i][0] = cur_pos[i]
            if cur_pos[i] > bounds[i][1]:
                bounds[i][1] = cur_pos[i]
    for g in gs:
        cur_ang += g.relative_angle
        cur_pos[0] += g.relative_ratio * math.cos(cur_ang)
        cur_pos[1] += -g.relative_ratio * math.sin(cur_ang)
        update_bounds()
    dims = (bounds[0][1] - bounds[0][0], bounds[1][1] - bounds[1][0])
    largest_dim = max(dims)
    draw_scale_factor = box_size / largest_dim
    offset = (abs(bounds[0][0]) * draw_scale_factor, abs(bounds[1][0]) * draw_scale_factor)

    # Draw gesture
    cur_pos = offset
    cur_ang = 0
    for g in gs:
        cur_ang += g.relative_angle
        x_delta = draw_scale_factor * g.relative_ratio * math.cos(cur_ang)
        y_delta = -draw_scale_factor * g.relative_ratio * math.sin(cur_ang)
        next_pos = (cur_pos[0] + x_delta, cur_pos[1] + y_delta)

        pygame.draw.line(surf, (255, 0, 0), cur_pos, next_pos, 3)

        cur_pos = next_pos

    return surf


def make_gesture():
    while True:
        filename = input("Enter a filename (omit .gest suffix): ").strip().lower().replace(" ", "_")
        if filename + ".gest" in os.listdir():
            print("File already exists, try again buddy")
        else:
            print(f"Your gesture will be saved as '{filename}.gest'")
            break

    points = []
    def loop(events: list[[pygame.event.Event]], screen: pygame.Surface) -> bool:
        for e in events:
            if e.type == pygame.MOUSEBUTTONUP:
                points.append((e.pos[0], -e.pos[1]))
            elif e.type == pygame.QUIT:
                return len(points) <= 1
        for pt in points:
            pygame.draw.circle(screen, (255, 0, 0), (pt[0], -pt[1]), 5)
        return True
    draw_pygame_realtime(loop)

    total_length = slice_dist(points, 1, len(points))

    rel_angs = []
    percent_distances = []
    cur_angle = 0
    for p in range(1, len(points)):
        pure_ang = math.atan2(points[p][1] - points[p-1][1], points[p][0] - points[p-1][0])
        rel_angs.append(pure_ang - cur_angle)
        percent_distances.append(slice_dist(points, p, p+1) / total_length)
        cur_angle = pure_ang

    factor = 10 / min(percent_distances)
    ratios = [round(d * factor) for d in percent_distances]
    gest = [GestComp(rel_angs[i], ratios[i]) for i in range(len(ratios))]

    with draw_pygame((500, 500)) as screen:
        gsurf = visualize_gesture(gest)
        screen.blit(gsurf, gsurf.get_rect(center=(250, 250)))

    if input("Save Gesture? (y/n) ").strip().lower()[0] == 'y':
        with open(f"{filename}.gest", "wb") as file:
            pickle.dump(gest, file)


def angle_diff(ang1: float, ang2: float) -> float:
    """Return absolute value different between the angles in radians. Chooses the closest
    difference; i.e diff between 2pi - 1 and 0 is 1, not 2pi - 1"""
    min_ang = min(ang1, ang2)
    max_ang = max(ang1, ang2)

    return min(abs(max_ang - min_ang), abs(max_ang - (min_ang + 2*math.pi)))
