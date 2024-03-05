using System;
using System.Collections.Generic;
using UnityEngine;

public enum GestureSystemPriorityLayers {
    SPELLS = 0,
    UI = 1
}

public abstract class AGestureSystem: MonoBehaviour {

    /** If no gesture match was found on the entire priority layer, this event is fired */
    public event Action GestureFailEvent = delegate {};  public void invokeGestureFailEvent() { GestureFailEvent(); }
    
    /* Event triggered true when the player has dragged for a long enough distance that it would be considered a gesture upon mouse release.
    Event also triggered false when the gesture finishes drawing. This second trigger always comes with a corresponding fail, backfire, or success event,
    but you also get this event to know generally when the gesture is no longer being drawn*/
    public event Action beganDrawingEvent;  protected void invokeBeganDrawingEvent() { beganDrawingEvent(); }

    /** 
    * Enable drawing visuals and gesture recognition
    */
    public abstract void enableGestureDrawing();

    /**
    * Disable visuals and recognition
    */
    public abstract void disableGestureDrawing();

    /** Return if the system is currently enabled to draw and recognize gestures. Should be false initially */
    public abstract bool isEnabled();

    /** Appends a new registree and returns the registree object (for event listening). Priority layer means that only gestures at the highest
    priority layer are checked. All registrees in a higher layer must deregister for a lower layer to start being recognized. This is useful since you
    might not want spell gestures to be recognized in a UI */
    public abstract GestureSysRegistree RegisterNewListener(int priorityLayer);

    /** Provide the registreeId of the GestureSysRegistree that you'd like removed. */
    public abstract void DeRegisterListener(int registreeId);
}
