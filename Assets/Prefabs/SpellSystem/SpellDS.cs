using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/**
* Data structure storing relevant spell details. Instantiated from a tree config class which has access
* to prefab injection. Tied to a specific SpellParams object which must be passed to the SpellPrefab on instantiation
*/
public class SpellDS {
    
    public string Name {get; private set;}
    public GameObject SpellPrefab {get; private set;}
    //public GameObject AimSystemPrefab {get; private set;}
    public Gesture Gesture {get; private set;}
    public int NumCharges {get; private set;}
    public SpellElementColorPalette ColorPalette {get; private set;}

    public SpellDS(GameObject spellPrefab, Gesture gesture, int numCharges, SpellElementColorPalette colorPalette, string name = "Unnamed Spell") {
        SpellPrefab = spellPrefab;
        //AimSystemPrefab = aimSystemPrefab;
        Gesture = gesture;
        Name = name;
        NumCharges = numCharges;
        ColorPalette = colorPalette;
    }
}
