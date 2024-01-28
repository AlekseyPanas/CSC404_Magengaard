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


@dataclass
class GestComp:
    """
    A gesture consists of relative components. Angle in radians and ratio is any int
    """
    relative_angle: float
    relative_ratio: int


def fair_segment_distance(points: list[tuple[float, float]], b: int, e: int, division_factor: float) -> float:
    """
    If the first and last point in points form a line segment, compute how far off all the points are from being
    on that line segment
    :param points: A sequence of points
    :param b: index to start computing
    :param e: index to end computing
    :return: Return a value on a 0-1 scale where 1 is perfect and 0 is absolutely wrong
    """
    def dist_from_line(lna: tuple[float, float], lnb: tuple[float, float], pt: tuple[float, float]) -> float:
        """Return the distance from pt to the closest point on line lna,lnb"""
        if pt == lna or pt == lnb: return 0

        lna = np.array(lna)
        lnb = np.array(lnb)
        pt = np.array(pt)

        ln_ab_vec = lnb - lna
        lna_pt_vec = pt - lna
        normalized_line_vec = ln_ab_vec / np.linalg.norm(ln_ab_vec)

        projected_magnitude = np.dot(normalized_line_vec, lna_pt_vec).item()
        closest_pt = lna + (normalized_line_vec * projected_magnitude)

        # draw_user_points([(p[0], -p[1]) for p in [closest_pt, lna, lnb, pt]])

        return np.linalg.norm(pt - closest_pt)

    # print(dist_from_line((50, 300), (450, 100), (100, 50)))
    # print(dist_from_line((50, 300), (450, 100), (150, 90)))

    distances = [dist_from_line(points[b], points[e - 1], points[i]) for i in range(b, e)]

    return distances[len(distances) // 2] / division_factor


def slice_dist(pts: list[tuple[float, float]], b: int, e: int):
    """Compute total length of a subsection of a sequence pts
    :param b: lower bound 1 <= b
    """
    length = 0
    for i in range(b, e):
        length += math.dist(pts[i], pts[i - 1])
    return length


@contextmanager
def draw_pygame(size=(500, 500)):
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


def visualize_gesture(gs: list[GestComp]):
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


def angle_diff(ang1: float, ang2: float) -> float:
    """Return absolute value different between the angles in radians. Chooses the closest
    difference; i.e diff between 2pi - 1 and 0 is 1, not 2pi - 1"""
    min_ang = min(ang1, ang2)
    max_ang = max(ang1, ang2)

    return min(abs(max_ang - min_ang), abs(max_ang - (min_ang + 2*math.pi)))


def compare_seq_to_gesture(pts: list[tuple[int, int]], gs: list[GestComp],
                           ratio_prediction_checks=tuple(),
                           minimization_weights=(1, 3, 3, 1),
                           debug=False) -> float:
    """
    Return a total error value between the pts and give gesture gs

    :param gs: the gesture to compare against
    :param pts: the user drawn gesture
    :param ratio_prediction_checks: When minimizing for the perfect segment in the user made gesture, we take into
    account the error of angle to the next segment. But because we don't know the size of the current segment, we
    also don't know where the second one is. Thus, we approximate this angle by taking the point that is ahead
    distance-wise by the perfect ratio of the subsequent segment, but also checking a few extra points. This tuple
    should have positive or negative percentage float values indicating the discrete +- checks to make from the pure
    ratio
    :param minimization_weights: The algorithm works by attempting to find the best segmentation of the user made pts
    to minimize the error with respect to the real gesture. To do this, the algorithm probes the user gesture for every
    segment attempting to find the best next segment by finding the minimum of 4 components: the error of the angle
    wrt to the previous component, the fair segment distance, the ratio error wrt to pure gesture, and an angle error estimate
    to the next segment. This tuple gives the weights of each error component respectively
    :param debug: Show plots, show pygame
    """
    # User drawing calculation variables will be prefixed with u_, gesture stuff with g_

    # Remove duplicate points and negate y coord since pygame is reversed to math
    pts = [(pts[p][0], -pts[p][1]) for p in range(len(pts)) if p == 0 or tuple(pts[p-1]) != tuple(pts[p])]

    ratio_prediction_checks = list(ratio_prediction_checks) + [0]

    def compute_point_closest_to_dist(b: int, dist: float):
        """Return the index of the point that is as close as possible
         to being dist away from the point at index b along the path length
        """
        cur_dist = 0
        for i in range(b+1, len(pts)):
            cur_dist += math.dist(pts[i], pts[i - 1])
            if cur_dist >= dist:
                return i
        return len(pts) - 1

    @dataclass
    class CompErr:
        idx: int  # Index of point in pts
        ang: float  # The angle of the segment in radians
        ang_e: float  # Angle error with previous component
        fsd: float  # Fair segment distance
        ratio_e: float  # Ratio error

    u_length = slice_dist(pts, 1, len(pts))  # Total length of the user drawing using Euclidean distance between mouse points

    g_ratio_total = sum([g.relative_ratio for g in gs])
    g_percent_ratios = [g.relative_ratio / g_ratio_total for g in gs]  # Gesture component ratios in percentage form

    # - For each new segment
    #   - Vary ratio and find the minimum point between some weighed sum of (error angle with prev gest), (fair segment distance), (ratio error), and
    #     (min angle error with respect to subsequent component estimated via precise ratio and some number of error values)
    #           - For a perfectly drawn gesture, this minimum will reside at the precise point where two components connect
    comp_errs: list[CompErr] = []
    for i in range(len(gs)):
        prev_ang = comp_errs[-1].ang if len(comp_errs) else 0  # Pure angle of the previous parsed user component
        prev_pt = pts[comp_errs[-1].idx] if len(comp_errs) else pts[0]  # (x,y) point of previous gesture's endpoint
        start_pt_idx = comp_errs[-1].idx + 1 if len(comp_errs) else 1  # The index at which to start looping points

        p_errs = []  # Accumulates point error values to find the best matched point for the next component

        # Plotting stuff
        plt_idxs = []
        plt_ang_wrt_prev = []
        plt_fsd = []
        plt_ratio_err = []
        plt_ang_wrt_next = []
        plt_sums = []

        for p in range(start_pt_idx, len(pts)):
            if i == len(gs) - 1: p = len(pts) - 1  # No probing on final iteration

            # Compute angle of next segment if it were to end at p
            pure_ang = math.atan2(pts[p][1] - prev_pt[1], pts[p][0] - prev_pt[0])
            # Compute angle difference with previous component assuming this component ends at p
            ang_err_wrt_prev = angle_diff(gs[i].relative_angle, pure_ang - prev_ang) * minimization_weights[0]
            # Compute fsd for this component if it were to end at p
            fsd = fair_segment_distance(pts, start_pt_idx, p+1, u_length) * minimization_weights[1]
            # Compute ratio error with respect to actual gesture if this component were to end at p
            ratio_err = abs((slice_dist(pts, start_pt_idx, p+1) / u_length) - g_percent_ratios[i]) * minimization_weights[2]

            # For last iteration, simply add error values, no point probing
            if i == len(gs) - 1: comp_errs.append(CompErr(p, pure_ang, ang_err_wrt_prev, fsd, ratio_err)); break;

            # The indexes of all estimations of the next component for angle checking
            next_seg_est_idxs = [compute_point_closest_to_dist(
                p, max(min(  # Clamp, effectively
                    (gs[i+1].relative_ratio / sum([gs[g].relative_ratio for g in range(i+1, len(gs))]))  # percent ratio of next segment relative to ratios of all remaining unlooped segments
                    + r, 1), 0) * slice_dist(pts, p+1, len(pts))  # Remaining distance
            ) for r in ratio_prediction_checks]
            # Among all next component estimates, find the smallest angle error with respect to the current component if it were to end at p
            ang_err_wrt_next_best_est = min([angle_diff(
                math.atan2(pts[n][1] - pts[p][1], pts[n][0] - pts[p][0]) - pure_ang, gs[i+1].relative_angle
            ) for n in next_seg_est_idxs]) * minimization_weights[3]

            # Sum the weighed components and append them
            p_err = sum([ang_err_wrt_prev, fsd, ratio_err, ang_err_wrt_next_best_est])
            p_errs.append((p_err, p, pure_ang, ang_err_wrt_prev, fsd, ratio_err))

            # Add to plotting data
            if debug:
                plt_idxs.append(p)
                plt_ang_wrt_prev.append(ang_err_wrt_prev)
                plt_fsd.append(fsd)
                plt_ratio_err.append(ratio_err)
                plt_ang_wrt_next.append(ang_err_wrt_next_best_est)
                plt_sums.append(p_err)

        # Visually plot the graphs of the components to visualize which point ends up being the minimum (i.e the point estimated to be the next component)
        if debug:
            plts_lst = [plt_ang_wrt_prev, plt_fsd, plt_ratio_err, plt_ang_wrt_next, plt_sums]
            for plt_dat in plts_lst: plt.plot(plt_idxs, plt_dat)
            plt.legend(["ang_prev", "fsd", "ratio_err", "ang_next", "total"])
            plt.show()

        if i < len(gs) - 1:  # Skip minimization if last iteration
            if not len(p_errs):  # If previous components already exhausted all points, add a max error component for remaining comps
                comp_errs.append(CompErr(len(pts) - 1, 0, math.pi * 2, 0, g_percent_ratios[i]))
            else:
                tup_of_min_err_pt = min(p_errs, key=lambda tp: tp[0])  # The next point which yields minimum error, will be used as next gesture point
                comp_errs.append(CompErr(tup_of_min_err_pt[1], tup_of_min_err_pt[2], tup_of_min_err_pt[3], tup_of_min_err_pt[4], tup_of_min_err_pt[5]))

        if debug: draw_user_points(pts[:comp_errs[-1].idx + 1])

    # Sum the weighed errors of each component and find the average weighed error
    weighed_errs = [c.ang_e + c.fsd + c.ratio_e for c in comp_errs]
    avg_weighed_err = sum(weighed_errs) / len(weighed_errs)
    if debug:
        print(weighed_errs)
        print(comp_errs)
        print(avg_weighed_err)
    return avg_weighed_err


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


def draw_pygame_realtime(loop_body: Callable[[list[pygame.event.Event], pygame.Surface], bool], size=(500, 500)):
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


def get_user_drawn_points():
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


def draw_user_points(pts):
    with draw_pygame() as screen:
        for p in pts:
            pygame.draw.circle(screen, (255, 0, 0), (p[0], -p[1]), 3)


def run_multi_matching_demo(filenames: list[str]):
    """
    Run a demo where user draws gestures and it displays the best match accuracy among provided gesture files
    :param filenames: list of file names without ".gest" suffix
    """

    gests = []
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

    while running:
        screen.fill((255, 255, 255))

        for e in pygame.event.get():
            if e.type == pygame.QUIT:
                running = False
            elif e.type == pygame.MOUSEBUTTONDOWN:
                track = True
            elif e.type == pygame.MOUSEBUTTONUP:
                track = False

                if len(set(user_sample)) > 5:

                    results = []
                    total_time = 0
                    for g in gests:
                        runtime = timeit.timeit("results.append(compare_seq_to_gesture(user_sample, g))", globals=locals() | globals(), number=1)
                        print(f"Runtime: {runtime}")
                        total_time += runtime
                    best_match_idx = min(enumerate(results), key=lambda x: x[1])[0]
                    matched_gest = best_match_idx
                    print(f"Matched Gesture '{filenames[best_match_idx]}'  -----  Accuracy {results[best_match_idx]}  ----- Total Runtime {total_time}")

                user_sample = []

        if matched_gest is not None:
            screen.blit(surfs[matched_gest], surfs[matched_gest].get_rect(center=(250, 250)))

        if track:
            user_sample.append(tuple(pygame.mouse.get_pos()))
        for p in user_sample:
            pygame.draw.circle(screen, (255, 0, 0), p, 4)

        pygame.display.update()

        clock.tick(60)


if __name__ == "__main__":
    #make_gesture()

    run_multi_matching_demo(["g1", "g2", "g3", "g4"])

    #g1 = [GestComp(math.pi / 10, 3), GestComp(math.pi / 8, 3), GestComp(math.pi / 6, 2), GestComp(math.pi / 2, 2), GestComp(math.pi / 2, 2)]
