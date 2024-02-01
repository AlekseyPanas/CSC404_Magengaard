using System.Collections.Generic;

public delegate void GestureSuccess(int index);

public interface IGestureSystem {

    /* Event triggered when a gesture from the set sequence was successfully drawn. The index provided
    corresponds to the list set in the setGesturesToRecognize method */
    public event GestureSuccess GestureSuccessEvent;

    /** 
    * Enable drawing visuals and gesture recognition
    */
    void enableGestureDrawing();

    /**
    * Disable visuals and recognition
    */
    void disableGestureDrawing();

    /**
    * Set a sequence of gestures that the system should match for. Matches send an event with an index
    * into this list
    */
    void setGesturesToRecognize(List<List<GestureUtils.GestComp>> gestures);
}
