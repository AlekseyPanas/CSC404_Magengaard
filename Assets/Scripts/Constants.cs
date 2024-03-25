
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

    /** 
    * Given a list of 2D projected vertices and an extrusion height, return a mesh of a 
    * vertically extruded convex hull encompassing the given points. Also return a list
    * of the convex hull vertices in flattened form
    */
    public static Tuple<Mesh, List<Vector2>> GetExtrudedConvexHullFromMeshProjection(List<Vector2> verts2D, float extrusionHeight) {
        Mesh m = new Mesh();

        // Compute convex hull vertices
        List<Vector2> hullVerts2D = new();
        List<Vector3> hullVerts = new();
        List<int> hullIdxs = ComputeConvexHull(verts2D);
        List<Vector3> normals = new();
        List<Vector2> uv = new();
        foreach (var i in hullIdxs) { 
            hullVerts2D.Add(verts2D[i]);
            hullVerts.Add(new Vector3(verts2D[i].x, extrusionHeight / 2, verts2D[i].y)); 
            normals.Add(Vector3.up);
            uv.Add(verts2D[i].normalized * 0.3f);
        }
        foreach (var i in hullIdxs) { 
            hullVerts.Add(new Vector3(verts2D[i].x, -extrusionHeight / 2, verts2D[i].y)); 
            normals.Add(Vector3.down);
            uv.Add(verts2D[i].normalized * 0.3f);
        }

        m.vertices = hullVerts.ToArray();

        // Compute triangles
        List<int> triangles = new();
        

        for (int i = 0; i < hullIdxs.Count - 2; i++) {
            triangles.Add(0);
            triangles.Add(i+1);
            triangles.Add(i+2);

            triangles.Add(hullIdxs.Count);
            triangles.Add(hullIdxs.Count+i+2);
            triangles.Add(hullIdxs.Count+i+1);
        }

        for (int i = 0; i < hullIdxs.Count; i++) {
            triangles.Add(i);
            triangles.Add(hullIdxs.Count + ((i+1) % hullIdxs.Count));
            triangles.Add((i+1) % hullIdxs.Count);

            triangles.Add(i);
            triangles.Add(hullIdxs.Count + i);
            triangles.Add(hullIdxs.Count + ((i+1) % hullIdxs.Count));
        }

        m.triangles = triangles.ToArray();
        m.normals = normals.ToArray();
        m.uv = uv.ToArray();

        return new Tuple<Mesh, List<Vector2>>(m, hullVerts2D);
    }

    /** 
    * Given a mesh, retrieve all vertices projected to the flat xz plane, transformations included
    */
    public static List<Vector2> MeshToFlatVertices(GameObject objectToProject) {
        Mesh meshToProject = objectToProject.GetComponent<MeshFilter>().mesh;
        Transform trans = objectToProject.transform;

        // Apply the object's transformation
        var verts = meshToProject.vertices;
        List<Vector3> transformedVerts = new();
        foreach (var vert in verts) {
            Vector3 scaled = new Vector3(vert.x * trans.localScale.x, vert.y * trans.localScale.y, vert.z * trans.localScale.z);
            Vector3 rotated = trans.rotation * scaled;
            transformedVerts.Add(rotated);
        }

        // Flatten Vectors
        List<Vector2> verts2D = new();
        foreach (var vert in transformedVerts) {
            Vector2 flattened = new Vector2(vert.x, vert.z);
            if (!verts2D.Contains(flattened)) {
                verts2D.Add(flattened);
            }
        }
        
        return verts2D;
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

            // Find point that matches the smallest angle of motion from the current segment
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
