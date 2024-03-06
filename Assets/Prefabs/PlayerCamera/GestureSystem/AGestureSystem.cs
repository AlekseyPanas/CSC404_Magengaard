using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum GestureControllablePriorities {
    PICKUP = 1,
    DEATH = 2,
    CUTSCENE = 3
}

public abstract class AGestureSystem: AGestureSystemControllable {

    private List<Gesture> _gesturesToRecognize;

    public ReadOnlyCollection<Gesture> GesturesToRecognize {get {
        return _gesturesToRecognize.AsReadOnly();
    }}

    /** Set the gestures to recognize. The gesture system will now match gestures from this list every time a drawing is made */
    public void SetGesturesToRecognize(List<Gesture> gestures) { _gesturesToRecognize = gestures; }

    /** Remove all gestures to recognize. The system will not match nothing */
    public void clearGesturesToRecognize() { _gesturesToRecognize = new(); }

    /** 
    * Enable drawing visuals and gesture recognition. 
    */
    public abstract void enableGestureDrawing();

    /**
    * Disable visuals and recognition. Unlike clearing the gestures to recognize, this method also disables all drawing visuals. It does not clear the list
    * of gestures to recognize so upon re-enabling, that list should be functional once again
    */
    public abstract void disableGestureDrawing();

    /** Return if the system is currently enabled to draw and recognize gestures. Should be false initially */
    public abstract bool isEnabled();

    /** When a new controller takes over, clears gestures and enables drawing */
    protected override void OnControllerChange() {
        clearGesturesToRecognize();
        enableGestureDrawing();
    }

    protected override AGestureSystem ReturnSelf() { return this; }
}
