using System.Collections.Generic;
using UnityEngine;

public delegate void GestureSuccess(int index);
public delegate void GestureBackfire(int index);
public delegate void GestureFail();

public interface IGestureSystem {

    /* Event triggered when a gesture from the set sequence was drawn at a particular accuracy threshold. The index provided
    corresponds to the list set in the setGesturesToRecognize method. */
    public event GestureSuccess GestureSuccessEvent;  // A returned index of -1 indicates a tap
    public event GestureBackfire GestureBackfire;
    public event GestureFail GestureFail;

    /** 
    * Enable drawing visuals and gesture recognition
    */
    public abstract void enableGestureDrawing();

    /**
    * Disable visuals and recognition
    */
    public abstract void disableGestureDrawing();

    /**
    * Set a sequence of gestures that the system should match for. Matches send an event with an index
    * into this list
    */
    public abstract void setGesturesToRecognize(List<Gesture> gestures);

    /**
    * Remove all gesture recognition. Drawing is still enabled but only taps will be registered
    */
    public abstract void clearGesturesToRecognize();

    /** Return if the system is currently enabled to draw and recognize gestures. Should be false initially */
    public abstract bool isEnabled();
}
