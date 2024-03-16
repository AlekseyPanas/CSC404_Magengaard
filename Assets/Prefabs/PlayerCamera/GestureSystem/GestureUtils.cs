using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;


/**
* A singular component of a gesture represented by a relative size ratio and a relative angle. A gesture is then
* an ordered sequence of these components.
*/
public struct GestComp {
    public GestComp(float rel_ang, float rel_ratio) {
        RelAng = rel_ang;
        RelRatio = rel_ratio;
    }

    public float RelAng {get;}
    public float RelRatio {get;}
}


public class GestureUtils {
    

    /**
     * Helper struct for the compare_seq_to_gest algorithm. Used to track errors of segmented user components
     */
    private struct CompErr {
        public CompErr(int idx, float ang, float ang_e, float fsd, float ratio_e) {
            Idx = idx;
            Ang = ang;
            AngE = ang_e;
            Fsd = fsd;
            RatioE = ratio_e; 
        }

        public int Idx {get;}  // Index in user points list
        public float Ang {get;}  // Pure angle of component segment in radians
        public float AngE {get;}  // Radians angle error with respect to given gesture
        public float Fsd {get;}  // Fair segment distance of component
        public float RatioE {get;}  // Ratio error with respect to given gesture
    }

    /**
     * Return p's distance from line ab
     */
    public static float dist_from_line(Vector2 a, Vector2 b, Vector2 p) {
        if (a.Equals(p) || b.Equals(p)) {return 0.0f;}

        Vector2 ab_vec = b - a;
        Vector2 ap_vec = p - a;
        float projected_magnitude = Vector2.Dot(ab_vec.normalized, ap_vec);
        Vector2 closest_pt_on_line = a + (ab_vec.normalized * projected_magnitude);

        return (closest_pt_on_line - p).magnitude;
    }

    /**
     * If the first and last point in points form a line segment, compute how far off all the points are from being
        on that line segment by returning the average distance from the segment divided by the division_factor
        :param points: A sequence of points
        :param b: index to start computing
        :param e: index to end computing
        :param division_factor: Divide final average by
        :return: Return a value on a 0-1 scale where 1 is perfect and 0 is absolutely wrong
     */
    public static float fair_segment_distance(List<Vector2> points, int b, int e, float division_factor) {
        List<float> distances = new List<float>();
        for (int i = b; i < e; i++) {
            distances.Add(dist_from_line(points[b], points[e-1], points[i]));
        }

        return distances[(int)(distances.Count * (2f/3f))] / division_factor;
    }

    /**
     * Return the path length of points in pts within the index range [b,e)
     * Be sure to provide b > 0 since the algo compares to the previous point at index b-1
     */
    public static float slice_distance(List<Vector2> pts, int b, int e) {
        float path_length = 0;
        for (int i = b; i < e; i++) {
            path_length += (pts[i] - pts[i-1]).magnitude;
        }
        return path_length;
    }

    /**
     * Given a sequence of points (pts), and a starting index (b), find the index i > b of a point in pts
     * such that the path length of pts[b:i+1] is closer to dist than for any other index
     */
    private static int closest_index_to_dist(List<Vector2> pts, int b, float dist) {
        float cur_dist = 0;
        for (int i = b+1; i < pts.Count; i++) {
            cur_dist += (pts[i-1] - pts[i]).magnitude;
            if (cur_dist > dist) {return i;}
        } return pts.Count - 1;  // Return last point in sequence if none found
    }

    /**
     * Return the total error value between the user drawn pts and the gesture given by gs.
     * The function attempts to find the optimal segmentation of the user points to form an equal number of components as gs.
     * Then for each estimated user component, it compares to the optimal gesture in the following three metrics:
     *      1. relative angle error: difference between relative angle of this component in gs and the estimated user one
     *      2. fair segment distance (fsd): the median distance of the points in this component from the line segment formed by 
     *                      the start and end point of this component
     *      3. ratio error: The percentage difference between the optimal percent length of this segment in gs and the one in 
     *                      the user drawn gesture
     * For each estimated user component, these three metrics are summed using the final_weights, provided in the same order
     * This gives an error for each component. The average error across all components is returned as the gesture accuracy
     *
     * :param pts: User drawn points
     * :param prune_dist_percent: As a percentage of the entire user path length, points within this distance of other points will
     *                      be pruned. The higher the value, the less accurate the algorithm but the faster it runs
     * :param gs: The gesture being compared
     * :param minimization_weights: When estimating a segmentation of the user gesture, the algorithm loops through each component
     *                      in gs sequentially and probes the remaining points in pts for a point that will form the next segment.
     *                      It chooses a point by minimizing four quantities:
     *                               1. previous relative angle error: the angle error from the one specified in gs for this
     *                                                 component with respect to the previously calculated user component
     *                               2. fsd: The same value described earlier
     *                               3. ratio error: The same value described earlier
     *                               4. next relative angle error: the angle error with respect to the next component. Since this
     *                                                next component is not yet computed, we estimate its point location by using
     *                                                its ratio in gs.
     *                      These four quantities are summed with respective weights in this tuple.
     * :param ratio_prediction_checks: For estimating the next relative angle error during minimization, the function takes a minimum
     *                      of computed estimated errors for the optiomal next gesture ratio and +- the values in this list. The values
     *                      should be provided as percentage offsets (e.g [0.05, -0.05]) for a +5% and -5% check
     */
    public static float compare_seq_to_gesture(List<Vector2> pts, List<GestComp> gs, 
                                               List<float> ratio_prediction_checks, Tuple<float, float, float, float> minimization_weights,
                                               Tuple<float, float, float> final_weights, float prune_dist_percent) {
        ratio_prediction_checks.Add(0);  // Add default perfect ratio check
        
        float u_length = slice_distance(pts, 1, pts.Count);  // Length of user path
        
        // Prune points that are too close to each other (to optimize runtime)
        List<int> prune_idxs = prune_path_points(pts, u_length * prune_dist_percent);
        List<Vector2> new_points = new List<Vector2>();
        for (int i = 0; i < pts.Count; i++) {
            if (!prune_idxs.Contains(i)) {new_points.Add(pts[i]);}
        }
        pts = new_points;

        // Convert gesture relative ratios to their percent of the total ratio sum (e.g [1,2,1] => [0.25, 0.5, 0.5])
        float g_ratio_total = 0; for (int g = 0; g < gs.Count; g++) {g_ratio_total += gs[g].RelRatio;}  // Sum of all relative ratios
        List<float> g_percent_ratios = new List<float>(); foreach (GestComp g in gs) {g_percent_ratios.Add(g.RelRatio / g_ratio_total);}
        
        List<CompErr> comp_errs = new List<CompErr>();
        for (int i = 0; i < gs.Count; i++) {
            float prev_ang = comp_errs.Count > 0 ? comp_errs[comp_errs.Count - 1].Ang : 0f;  // Pure angle of previous user component
            Vector2 prev_pt = comp_errs.Count > 0 ? pts[comp_errs[comp_errs.Count - 1].Idx] : pts[0];  // Index of endpoint of previous user component
            int start_pt_idx = comp_errs.Count > 0 ? comp_errs[comp_errs.Count - 1].Idx + 1 : 1;  // First point to probe from, right after endpoint of last user component
        
            List<Tuple<float, CompErr>> p_errs = new List<Tuple<float, CompErr>>();  // Store each point's error and component
            for (int p = start_pt_idx; p < pts.Count; p++) {
                if (i == gs.Count - 1) {p = pts.Count - 1;}  // No probing on final iteration

                // Compute angle of next segment if it were to end at p
                float pure_ang = Vector2.SignedAngle(new Vector2(1, 0), pts[p] - prev_pt);
                
                // Compute angle difference with previous component assuming this component ends at p
                float ang_err_wrt_prev = Mathf.Abs(Mathf.DeltaAngle(gs[i].RelAng, pure_ang - prev_ang));
                // Compute fsd for this component if it were to end at p
                float fsd = fair_segment_distance(pts, start_pt_idx, p+1, u_length);
                // Compute ratio error with respect to actual gesture if this component were to end at p
                float ratio_err = Mathf.Abs((slice_distance(pts, start_pt_idx, p+1) / u_length) - g_percent_ratios[i]);

                // For last iteration, simply add error values, no point probing
                if (i == gs.Count - 1) {comp_errs.Add(new CompErr(p, pure_ang, ang_err_wrt_prev, fsd, ratio_err)); break;}

                // Indexes of points for all the estimates of the next component
                List<int> next_seg_est_idxs = new List<int>();
                for (int r = 0; r < ratio_prediction_checks.Count; r++) {
                    // Sum of remaining ratios
                    float total_remaining_ratios = 0; for (int g = i+1; g < gs.Count; g++) {total_remaining_ratios += gs[g].RelRatio;}
                    // percent of remaining ratio total that the next component ratio constitutes (with r region checking added)
                    float next_gest_frac = Mathf.Clamp((gs[i+1].RelRatio / total_remaining_ratios) + r, 0f, 1f);
                    // Path length of remaining points after p
                    float total_remaining_path_length = slice_distance(pts, p+1, pts.Count);
                    // Index of the estimated point of the next component segment
                    next_seg_est_idxs.Add(closest_index_to_dist(pts, p, total_remaining_path_length * next_gest_frac));
                }
                // For each next component estimate, compute the angle error it would have with the current component compared to the gs relation
                float ang_err_wrt_next_best_est = float.PositiveInfinity;
                foreach (int np in next_seg_est_idxs) {
                    // Next component estimate relative angle by comparing pure next angle with pure current angle
                    float next_est_relative_ang = Mathf.DeltaAngle(pure_ang, Vector2.SignedAngle(new Vector2(1, 0), pts[np] - pts[p]));
                    // Finds error by comparing with relative angle set in gs of next component
                    float next_est_ang_err = Mathf.Abs(Mathf.DeltaAngle(gs[i+1].RelAng, next_est_relative_ang));
                    // Update minimum
                    if (next_est_ang_err < ang_err_wrt_next_best_est) {ang_err_wrt_next_best_est = next_est_ang_err;}
                }

                // Compute weighed sum error and append comp data for this point
                float p_err = (ang_err_wrt_prev  * minimization_weights.Item1) + (fsd  * minimization_weights.Item2) + 
                              (ratio_err  * minimization_weights.Item3) + (ang_err_wrt_next_best_est * minimization_weights.Item4);
                p_errs.Add(new Tuple<float, CompErr>(p_err, new CompErr(p, pure_ang, ang_err_wrt_prev, fsd, ratio_err)));
            }

            if (i < gs.Count - 1) {  // Skips minimization if last gesture, it was already added
                if (p_errs.Count == 0) {  // If previous components already exhausted all points, add a max error component for remaining comps
                    comp_errs.Add(new CompErr(pts.Count - 1, 0f, 360, 0f, g_percent_ratios[i]));
                } else {
                    // Compute the point with the least error and add it as the next estimated component
                    Tuple<float, CompErr> tup_of_min_err = p_errs[0];
                    foreach (Tuple<float, CompErr> tp in p_errs) {if (tp.Item1 < tup_of_min_err.Item1) {tup_of_min_err = tp;}}
                    comp_errs.Add(tup_of_min_err.Item2);
                }
            }
        }

        // Compute the weighed sum of every estimated component and return the average as the accuracy
        List<float> weighed_errs = new List<float>();
        foreach (CompErr c in comp_errs) {
            weighed_errs.Add((c.AngE * final_weights.Item1) + (c.Fsd * final_weights.Item2) + (c.RatioE * final_weights.Item3));
        }
        return weighed_errs.Average();
    }

    /** 
     * Loop pts and track any that are within dist to the previous. Return the indexes of these points to indicate they
     * should be pruned.
     * :param pts: The sequence of path points
     * :param dist: Euclidean distance within which to prune
     */
    public static List<int> prune_path_points(List<Vector2> pts, float dist) {
        List<int> idxs = new List<int>();
        int skip = 0;
        for (int i = 1; i < pts.Count; i++) {
            if ((pts[i-1-skip] - pts[i]).magnitude <= dist) {
                idxs.Add(i);
                skip++;
            } else {skip = 0;}
        }
        return idxs;
    }
}
