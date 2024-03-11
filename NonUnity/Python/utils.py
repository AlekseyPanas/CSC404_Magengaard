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


def angle_diff(ang1: float, ang2: float) -> float:
    """Return absolute value different between the angles in radians. Chooses the closest
    difference; i.e diff between 2pi - 1 and 0 is 1, not 2pi - 1"""
    min_ang = min(ang1, ang2)
    max_ang = max(ang1, ang2)

    return min(abs(max_ang - min_ang), abs(max_ang - (min_ang + 2*math.pi)))
