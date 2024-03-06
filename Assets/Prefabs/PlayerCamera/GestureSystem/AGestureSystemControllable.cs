using System;

public abstract class AGestureSystemControllable: AControllable<AGestureSystem> {
    protected GestureSystemControllerRegistrant GetDefaultControllerCasted() { return (GestureSystemControllerRegistrant)_defaultController; }
    protected GestureSystemControllerRegistrant GetCurrentControllerCasted() { return (GestureSystemControllerRegistrant)_currentController; }
    public new GestureSystemControllerRegistrant RegisterController(int priority) { return (GestureSystemControllerRegistrant)base.RegisterController(priority); }
    public new GestureSystemControllerRegistrant RegisterDefault() { return (GestureSystemControllerRegistrant)base.RegisterDefault(); }

    public class GestureSystemControllerRegistrant: ControllerRegistrant {
        public GestureSystemControllerRegistrant(AGestureSystemControllable controllable): base(controllable) {}

        /* Event triggered when a gesture from the set sequence was drawn at a particular accuracy threshold. The index provided
        corresponds to the list set in the setGesturesToRecognize method. */
        public event Action<int> GestureSuccessEvent = delegate {};  
        public void invokeGestureSuccessEvent(int index) { GestureSuccessEvent(index); }

        public event Action<int> GestureBackfireEvent = delegate {};  
        public void invokeGestureBackfireEvent(int index) { GestureBackfireEvent(index); }

        /** If a drawing was made but no gesture matched within a valid accuracy threshold, this event gets fired */
        public event Action GestureFailEvent = delegate {};  
        public void invokeGestureFailEvent() { GestureFailEvent(); }
        
        /* Event triggered true when the player has dragged for a long enough distance that it would be considered a gesture upon mouse release.
        Event also triggered false when the gesture finishes drawing. This second trigger always comes with a corresponding fail, backfire, or success event,
        but you also get this event to know generally when the gesture is no longer being drawn*/
        public event Action beganDrawingEvent;  
        public void invokeBeganDrawingEvent() { beganDrawingEvent(); }
    }
}
