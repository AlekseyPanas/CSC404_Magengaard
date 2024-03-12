
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

    public static readonly float SPELL_SPAWN_DISTANCE_FROM_PLAYER = 0.5f;
    public static readonly Tuple<float, float, float> FINAL_WEIGHTS = new Tuple<float, float, float>(Mathf.PI / 360f, 3f, 3f);
    public static readonly Tuple<float, float, float, float> MINIMIZATION_WEIGHTS = new Tuple<float, float, float, float>(Mathf.PI / 360f, 3f, 3f, Mathf.PI / 360f);
    public static readonly List<float> NEXT_CHECKS = new List<float>();
}
