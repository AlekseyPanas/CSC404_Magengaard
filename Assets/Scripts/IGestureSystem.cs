using System.Collections.Generic;
using UnityEngine;

public delegate void GestureSuccess(int index);
public delegate void GestureBackfire(int index);
public delegate void GestureFail();
public delegate void IsDrawing(bool isDrawing);

public interface IGestureSystem {

    /* Event triggered when a gesture from the set sequence was drawn at a particular accuracy threshold. The index provided
    corresponds to the list set in the setGesturesToRecognize method. */
    public event GestureSuccess GestureSuccessEvent;
    public event GestureBackfire GestureBackfire;
    public event GestureFail GestureFail;
    /* Event triggered true when the player has dragged for a long enough distance that it would be considered a gesture upon mouse release.
    Event also triggered false when the gesture finishes drawing. This second trigger always comes with a corresponding fail, backfire, or success event,
    but you also get this event to know generally when the gesture is no longer being drawn*/
    public event IsDrawing isDrawingEvent; 

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
