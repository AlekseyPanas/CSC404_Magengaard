
using System;
using System.Collections.Generic;
using UnityEngine;

class Const {
    public static readonly float SPELL_SPAWN_DISTANCE_FROM_PLAYER = 1f;
    public static readonly Tuple<float, float, float> FINAL_WEIGHTS = new Tuple<float, float, float>(Mathf.PI / 360f, 30f, 3f);
    public static readonly Tuple<float, float, float, float> MINIMIZATION_WEIGHTS = new Tuple<float, float, float, float>(Mathf.PI / 360f, 3f, 3f, Mathf.PI / 360f);
    public static readonly List<float> NEXT_CHECKS = new List<float>();
}
