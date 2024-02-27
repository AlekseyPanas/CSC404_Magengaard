using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/** Object provided and stored by the gesture system when an external class registers itself to listen for gestures */
public class GestureSysRegistree {
    
    // Configures a unique ID for each registree (used to delete registrees)
    private static int nextFreeCount = 0;
    public GestureSysRegistree() {
        registreeId = nextFreeCount;
        nextFreeCount++;
    }
    public readonly int registreeId;

    private List<Gesture> _gesturesToRecognize;
    public ReadOnlyCollection<Gesture> GesturesToRecognize {get {
        return _gesturesToRecognize.AsReadOnly();
    }}

    /** Set the gestures to recognize for this registree */
    public void SetGesturesToRecognize(List<Gesture> gestures) { _gesturesToRecognize = gestures; }

    /** Remove all gesture recognition from this registree */
    public void clearGesturesToRecognize() { _gesturesToRecognize = new(); }

    /* Event triggered when a gesture from the set sequence was drawn at a particular accuracy threshold. The index provided
    corresponds to the list set in the setGesturesToRecognize method. */
    public event Action<int> GestureSuccessEvent = delegate {};  
    public void invokeGestureSuccessEvent(int index) { GestureSuccessEvent(index); }

    public event Action<int> GestureBackfireEvent = delegate {};  
    public void invokeGestureBackfireEvent(int index) { GestureBackfireEvent(index); }
}
