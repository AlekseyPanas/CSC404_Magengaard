from utils import *
from gest_base import *

class LinearNextApproxAlgo(Segmentator, ErrorFunction):
    """
    Segmentator which segments by minimizing accuracy via a linear search while approximating the next segment by
    optimal ratio


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
    def __init__(self, ratio_prediction_checks=tuple(),
                           minimization_weights=(1, 3, 3, 1),
                           debug=False):
        self.debug = debug
        self.minimization_weights = minimization_weights
        self.ratio_prediction_checks = ratio_prediction_checks

        self._err = None

    def fair_segment_distance(self, points: list[tuple[float, float]], b: int, e: int, division_factor: float) -> float:
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

    def get_segmentation(self, point_seq: list[tuple[int, int]], num_segments: int, gest: list[GestComp]) -> set[int]:
        ratio_prediction_checks = list(self.ratio_prediction_checks) + [0]

        def compute_point_closest_to_dist(b: int, dist: float):
            """Return the index of the point that is as close as possible
             to being dist away from the point at index b along the path length
            """
            cur_dist = 0
            for i in range(b + 1, len(point_seq)):
                cur_dist += math.dist(point_seq[i], point_seq[i - 1])
                if cur_dist >= dist:
                    return i
            return len(point_seq) - 1

        @dataclass
        class CompErr:
            idx: int  # Index of point in pts
            ang: float  # The angle of the segment in radians
            ang_e: float  # Angle error with previous component
            fsd: float  # Fair segment distance
            ratio_e: float  # Ratio error

        u_length = slice_dist(point_seq, 1,
                              len(point_seq))  # Total length of the user drawing using Euclidean distance between mouse points

        g_ratio_total = sum([g.relative_ratio for g in gest])
        g_percent_ratios = [g.relative_ratio / g_ratio_total for g in gest]  # Gesture component ratios in percentage form

        # - For each new segment
        #   - Vary ratio and find the minimum point between some weighed sum of (error angle with prev gest), (fair segment distance), (ratio error), and
        #     (min angle error with respect to subsequent component estimated via precise ratio and some number of error values)
        #           - For a perfectly drawn gesture, this minimum will reside at the precise point where two components connect
        comp_errs: list[CompErr] = []
        for i in range(len(gest)):
            prev_ang = comp_errs[-1].ang if len(comp_errs) else 0  # Pure angle of the previous parsed user component
            prev_pt = point_seq[comp_errs[-1].idx] if len(comp_errs) else point_seq[0]  # (x,y) point of previous gesture's endpoint
            start_pt_idx = comp_errs[-1].idx + 1 if len(comp_errs) else 1  # The index at which to start looping points

            p_errs = []  # Accumulates point error values to find the best matched point for the next component

            # Plotting stuff
            plt_idxs = []
            plt_ang_wrt_prev = []
            plt_fsd = []
            plt_ratio_err = []
            plt_ang_wrt_next = []
            plt_sums = []

            for p in range(start_pt_idx, len(point_seq)):
                if i == len(gest) - 1: p = len(point_seq) - 1  # No probing on final iteration

                # Compute angle of next segment if it were to end at p
                pure_ang = math.atan2(point_seq[p][1] - prev_pt[1], point_seq[p][0] - prev_pt[0])
                # Compute angle difference with previous component assuming this component ends at p
                ang_err_wrt_prev = angle_diff(gest[i].relative_angle, pure_ang - prev_ang) * self.minimization_weights[0]
                # Compute fsd for this component if it were to end at p
                fsd = self.fair_segment_distance(point_seq, start_pt_idx, p + 1, u_length) * self.minimization_weights[1]
                # Compute ratio error with respect to actual gesture if this component were to end at p
                ratio_err = abs((slice_dist(point_seq, start_pt_idx, p + 1) / u_length) - g_percent_ratios[i]) * \
                            self.minimization_weights[2]

                # For last iteration, simply add error values, no point probing
                if i == len(gest) - 1: comp_errs.append(CompErr(p, pure_ang, ang_err_wrt_prev, fsd, ratio_err)); break;

                # The indexes of all estimations of the next component for angle checking
                next_seg_est_idxs = [compute_point_closest_to_dist(
                    p, max(min(  # Clamp, effectively
                        (gest[i + 1].relative_ratio / sum([gest[g].relative_ratio for g in range(i + 1,
                                                                                             len(gest))]))  # percent ratio of next segment relative to ratios of all remaining unlooped segments
                        + r, 1), 0) * slice_dist(point_seq, p + 1, len(point_seq))  # Remaining distance
                ) for r in ratio_prediction_checks]
                # Among all next component estimates, find the smallest angle error with respect to the current component if it were to end at p
                ang_err_wrt_next_best_est = min([angle_diff(
                    math.atan2(point_seq[n][1] - point_seq[p][1], point_seq[n][0] - point_seq[p][0]) - pure_ang, gest[i + 1].relative_angle
                ) for n in next_seg_est_idxs]) * self.minimization_weights[3]

                # Sum the weighed components and append them
                p_err = sum([ang_err_wrt_prev, fsd, ratio_err, ang_err_wrt_next_best_est])
                p_errs.append((p_err, p, pure_ang, ang_err_wrt_prev, fsd, ratio_err))

                # Add to plotting data
                if self.debug:
                    plt_idxs.append(p)
                    plt_ang_wrt_prev.append(ang_err_wrt_prev)
                    plt_fsd.append(fsd)
                    plt_ratio_err.append(ratio_err)
                    plt_ang_wrt_next.append(ang_err_wrt_next_best_est)
                    plt_sums.append(p_err)

            # Visually plot the graphs of the components to visualize which point ends up being the minimum (i.e the point estimated to be the next component)
            if self.debug:
                plts_lst = [plt_ang_wrt_prev, plt_fsd, plt_ratio_err, plt_ang_wrt_next, plt_sums]
                for plt_dat in plts_lst: plt.plot(plt_idxs, plt_dat)
                plt.legend(["ang_prev", "fsd", "ratio_err", "ang_next", "total"])
                plt.show()

            if i < len(gest) - 1:  # Skip minimization if last iteration
                if not len(
                        p_errs):  # If previous components already exhausted all points, add a max error component for remaining comps
                    comp_errs.append(CompErr(len(point_seq) - 1, 0, math.pi * 2, 0, g_percent_ratios[i]))
                else:
                    tup_of_min_err_pt = min(p_errs, key=lambda tp: tp[
                        0])  # The next point which yields minimum error, will be used as next gesture point
                    comp_errs.append(
                        CompErr(tup_of_min_err_pt[1], tup_of_min_err_pt[2], tup_of_min_err_pt[3], tup_of_min_err_pt[4],
                                tup_of_min_err_pt[5]))

            if self.debug: draw_user_points(point_seq[:comp_errs[-1].idx + 1])

        # Sum the weighed errors of each component and find the average weighed error
        weighed_errs = [c.ang_e + c.fsd + c.ratio_e for c in comp_errs]
        avg_weighed_err = sum(weighed_errs) / len(weighed_errs)
        if self.debug:
            print(weighed_errs)
            print(comp_errs)
            print(avg_weighed_err)
        self._err = avg_weighed_err

        return set([comp_errs[c].idx for c in range(len(comp_errs) - 1)])

    def get_err(self, point_seq: list[tuple[int,int]], segmentation: set[int], gest: list[GestComp]) -> float:
        return self._err
