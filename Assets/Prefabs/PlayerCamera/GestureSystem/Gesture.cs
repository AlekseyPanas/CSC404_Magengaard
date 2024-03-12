using System.Collections.Generic;
using UnityEngine;

/**
* A struct to store information about a spell gesture.
*
* :attribute gest: Sequence of components; used by recognition algorithm to match user drawings to gestures
* :attribute SuccessAccuracy: the accuracy threshold below which (inclusive) the gesture successfully casts the spell
* :attribute BackfireFailAccuracy: the accuracy threshold below which (inclusive) the gesture causes the spell to backfire. Set a value below SuccessAccuracy (or just -1) to remove this feature
* :attribute StartLocation: The gesture must start at this location, specifies as a percentage of width and height respectively
* :attribute LocationMaxRadius: The gesture must start within this radius of the StartLocation as a percentage of screen size. -1 means the gesture is location invariant
*
* An accuracy worse than the max of the above two thresholds counts as a total fail
*/
public struct Gesture {

    private List<GestComp> gest;
    public IList<GestComp> Gest {
        get {return gest.AsReadOnly();}
    }

    public float Bin1Acc {get; private set;}
    public float Bin2Acc {get; private set;}
    public float Bin3Acc {get; private set;}
    public float Bin4Acc {get; private set;}
    //public float SuccessAccuracy {get; private set;}
    //public float BackfireFailAccuracy {get; private set;}
    public Vector2 StartLocation {get; private set;}
    public float LocationMaxRadius {get; private set;}

    /**
    * :param SuccessAccuracy: the accuracy threshold below which (inclusive) the gesture successfully casts the spell
    * :param BackfireFailAccuracy: the accuracy threshold below which (inclusive) the gesture causes the spell to backfire. Set a value below SuccessAccuracy (or just -1) to remove this feature
    */
    public Gesture (List<GestComp> gest, float[] binAccs, Vector2? startLocation = null, float locationMaxRadius = -1) {
        this.gest = gest;
        Bin1Acc = binAccs[0];
        Bin2Acc = binAccs[1];
        Bin3Acc = binAccs[2];
        Bin4Acc = binAccs[3];
        StartLocation = (Vector2)(startLocation == null ? Vector2.zero : startLocation);
        LocationMaxRadius = locationMaxRadius;
    }
}
