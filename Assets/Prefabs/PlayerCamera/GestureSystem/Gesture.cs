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

    public float SuccessAccuracy {get; private set;}
    public float BackfireFailAccuracy {get; private set;}
    public Vector2 StartLocation {get; private set;}
    public float LocationMaxRadius {get; private set;}

    /**
    * :param SuccessAccuracy: the accuracy threshold below which (inclusive) the gesture successfully casts the spell
    * :param BackfireFailAccuracy: the accuracy threshold below which (inclusive) the gesture causes the spell to backfire. Set a value below SuccessAccuracy (or just -1) to remove this feature
    */
    public Gesture (List<GestComp> gest, float successAccuracy, float backfireFailAccuracy, Vector2? startLocation = null, float locationMaxRadius = -1) {
        this.gest = gest;
        SuccessAccuracy = successAccuracy;
        BackfireFailAccuracy = backfireFailAccuracy;
        StartLocation = (Vector2)(startLocation == null ? Vector2.zero : startLocation);
        LocationMaxRadius = locationMaxRadius;
    }
}
