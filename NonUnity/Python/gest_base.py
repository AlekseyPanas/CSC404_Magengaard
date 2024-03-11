from dataclasses import dataclass
from abc import abstractmethod


@dataclass
class GestComp:
    """
    A gesture consists of relative components. Angle in radians and ratio is any int
    """
    relative_angle: float
    relative_ratio: int


class Segmentator:
    @abstractmethod
    def get_segmentation(self, point_seq: list[tuple[int, int]], num_segments: int, gest: list[GestComp]) -> set[int]:
        """
        Given a sequence of points (pointSeq), a number of segments to make (num_segments),
        and the optimal gesture (gest), return a set of indexes into the sequence at which the sequence is segmented.
        """


class ErrorFunction:
    @abstractmethod
    def get_err(self, point_seq: list[tuple[int,int]], segmentation: set[int], gest: list[GestComp]) -> float:
        """
        Given a sequence of points, a chosen segmentation, and an optimal gesture to compare to, return a float
        representing the error of the segmented point sequence to the actual gesture
        """


class PointPreProcessor:
    @abstractmethod
    def pre_process_points(self, pts: list[tuple[int, int]]) -> list[tuple[int, int]]:
        """
        Given a list of points, return a modified list. This could be a shorted list with some points pruned
        """


def run_algorithm(point_seq: list[tuple[int, int]], gest: list[GestComp],
                  segmentator: Segmentator, error_function: ErrorFunction,
                  points_pre_processor: PointPreProcessor | None = None) -> tuple[float, set[int]]:
    segmentation = segmentator.get_segmentation(point_seq, len(gest), gest)
    return error_function.get_err(
        point_seq,
        segmentation,
        gest), segmentation
