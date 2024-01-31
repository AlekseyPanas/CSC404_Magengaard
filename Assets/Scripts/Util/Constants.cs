
using System;
using System.Collections.Generic;
using UnityEngine;

class Const {
    public static readonly float SPELL_SPAWN_DISTANCE_FROM_PLAYER = 1f;

    public static readonly List<GestureUtils.GestComp> FireballGesture = new List<GestureUtils.GestComp>();
    public static readonly List<GestureUtils.GestComp> SandstormGesture = new List<GestureUtils.GestComp>();
    public static readonly List<GestureUtils.GestComp> ElectroSphereGesture = new List<GestureUtils.GestComp>();
    public static readonly List<GestureUtils.GestComp> EarthWallGesture = new List<GestureUtils.GestComp>();
    public static readonly Tuple<float, float, float> FINAL_WEIGHTS = new Tuple<float, float, float>(Mathf.PI / 360f, 30f, 3f);
    public static readonly Tuple<float, float, float, float> MINIMIZATION_WEIGHTS = new Tuple<float, float, float, float>(Mathf.PI / 360f, 3f, 3f, Mathf.PI / 360f);
    public static readonly List<float> NEXT_CHECKS = new List<float>();
    public static readonly List<List<GestureUtils.GestComp>> Gestures = new List<List<GestureUtils.GestComp>>() {FireballGesture, SandstormGesture, ElectroSphereGesture, EarthWallGesture};
    
    public static readonly Dictionary<List<GestureUtils.GestComp>, SpellFactory.SpellId> gestureToID = 
            new Dictionary<List<GestureUtils.GestComp>, SpellFactory.SpellId>() {
            {SandstormGesture, SpellFactory.SpellId.SANDSTORM}, 
            {FireballGesture, SpellFactory.SpellId.FIREBALL}, 
            {ElectroSphereGesture, SpellFactory.SpellId.ELECTROSPHERE},
            {EarthWallGesture, SpellFactory.SpellId.EARTHENWALL}
    };
    static Const () {
        FireballGesture.Add(new GestureUtils.GestComp(-120, 1));
        FireballGesture.Add(new GestureUtils.GestComp(120, 1));
        FireballGesture.Add(new GestureUtils.GestComp(120, 1));

        SandstormGesture.Add(new GestureUtils.GestComp(-45, 4));
        SandstormGesture.Add(new GestureUtils.GestComp(90, 2));
        SandstormGesture.Add(new GestureUtils.GestComp(90, 1));
        SandstormGesture.Add(new GestureUtils.GestComp(90, 1));
        SandstormGesture.Add(new GestureUtils.GestComp(90, 1));

        ElectroSphereGesture.Add(new GestureUtils.GestComp(45, 1));
        ElectroSphereGesture.Add(new GestureUtils.GestComp(-90, 1));

        EarthWallGesture.Add(new GestureUtils.GestComp(90, 1));
        EarthWallGesture.Add(new GestureUtils.GestComp(-90, 1));
        EarthWallGesture.Add(new GestureUtils.GestComp(-90, 1));
    }
}
