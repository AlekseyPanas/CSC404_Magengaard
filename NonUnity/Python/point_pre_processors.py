from utils import *
from gest_base import *


class RemoveDupesPointPreProcessor(PointPreProcessor):
    """
    Preprocesses points by removing all duplicates
    """
    def pre_process_points(self, pts: list[tuple[int, int]]) -> list[tuple[int, int]]:
        return [(pts[p][0], pts[p][1]) for p in range(len(pts)) if p == 0 or tuple(pts[p-1]) != tuple(pts[p])]


class InvertPointsPreProcessor(PointPreProcessor):
    def pre_process_points(self, pts: list[tuple[int, int]]):
        return [(p[0], -p[1]) for p in pts]
