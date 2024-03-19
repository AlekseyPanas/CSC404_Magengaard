
using System;
using System.Collections.Generic;
using UnityEngine;

class Const {
    // Given a Vector2 v representing horizontal motion where v.y is camera.forward and v.x is camera.right/left, return a Vector2(x,z) direction
    // in terms of pure world coordinates. The returned direction has a world space magnitude of one */
    public static Vector2 HorizontalCam2World(Camera cam, Vector2 camDirections) {
        var cameraForward = cam.transform.forward;
        var forward = new Vector2(cameraForward.x, cameraForward.z);
        var right = new Vector2(cameraForward.z, -cameraForward.x);  // Negative reciprocal for orthogonal right vector
        var result = forward * camDirections.y + right * camDirections.x;
        return result.normalized;
    }

    public static List<Vector2> GetExtrudedConvexHullFromMeshProjection(GameObject objectToProject, float extrusionHeight) {
        Mesh meshToProject = objectToProject.GetComponent<MeshFilter>().mesh;
        Transform trans = objectToProject.transform;

        var verts = meshToProject.vertices;
        List<Vector3> transformedVerts = new();
        foreach (var vert in verts) {
            Vector3 scaled = new Vector3(vert.x * trans.localScale.x, vert.y * trans.localScale.y, vert.z * trans.localScale.z);
            Vector3 rotated = trans.rotation * scaled;
            transformedVerts.Add(rotated);
        }

        List<Vector2> verts2D = new();
        foreach (var vert in transformedVerts) {
            Vector2 flattened = new Vector2(vert.x, vert.z);
            if (!verts2D.Contains(flattened)) {
                verts2D.Add(flattened);
            }
        }

        List<Vector2> xx = new();
        foreach (var i in ComputeConvexHull(verts2D)) { xx.Add(verts2D[i]); }
        return xx;

        // Mesh m = new Mesh();

        // List<Vector3> v = new(){
        //     new Vector3(0, 1, 0),
        //     new Vector3(-1, 0, 0),
        //     new Vector3(1, 0, 0)
        // };

        // m.vertices = v.ToArray();
        // m.triangles = new int[]{0, 1, 2};

        // return m;
    }

    /** Given an array of points, compute a sequence of indexes representing the convex hull wrapping all the points */
    public static List<int> ComputeConvexHull(List<Vector2> points) {
        List<int> indexes = new();

        // Find leftmost point and add it as first
        float minx = Mathf.Infinity; int minxIdx = 0;
        for (int p = 0; p < points.Count; p++) { if (points[p].x < minx) { minx = points[p].x; minxIdx = p; } }
        indexes.Add(minxIdx);

        while (indexes.Count < 2 || indexes[0] != indexes[indexes.Count - 1]) {
            float nextBestAngle = Mathf.Infinity;
            int bestPointIdx = 0;

            for (int p = 0; p < points.Count; p++) {
                if (p != indexes[indexes.Count - 1]) {
                    float nextAng;
                    if (indexes.Count == 1) {
                        nextAng = Mathf.Abs(Vector2.SignedAngle(Vector2.up, points[p] - points[indexes[0]]));
                    } else {
                        nextAng = Mathf.Abs(Vector2.SignedAngle(points[indexes[indexes.Count - 1]] - points[indexes[indexes.Count - 2]], 
                                                      points[p] - points[indexes[indexes.Count - 1]]));
                    }
                    
                    if (nextAng < nextBestAngle) {
                        nextBestAngle = nextAng;
                        bestPointIdx = p;
                    }

                }
            }

            indexes.Add(bestPointIdx);
        }

        indexes.RemoveAt(indexes.Count - 1);
        return indexes;
    }

    public static readonly float SPELL_SPAWN_DISTANCE_FROM_PLAYER = 0.5f;
    public static readonly Tuple<float, float, float> FINAL_WEIGHTS = new Tuple<float, float, float>(Mathf.PI / 360f, 3f, 3f);
    public static readonly Tuple<float, float, float, float> MINIMIZATION_WEIGHTS = new Tuple<float, float, float, float>(Mathf.PI / 360f, 3f, 3f, Mathf.PI / 360f);
    public static readonly List<float> NEXT_CHECKS = new List<float>();
}
