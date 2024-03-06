using System;

public abstract class AMovementControllable : AControllable<MovementControllable> {
    protected MovementControllerRegistrant GetDefaultControllerCasted() { return (MovementControllerRegistrant)_defaultController; }
    protected MovementControllerRegistrant GetCurrentControllerCasted() { return (MovementControllerRegistrant)_currentController; }
    public new MovementControllerRegistrant RegisterController(int priority) { return (MovementControllerRegistrant)base.RegisterController(priority); }
    public new MovementControllerRegistrant RegisterDefault() { return (MovementControllerRegistrant)base.RegisterDefault(); }
    public class MovementControllerRegistrant: ControllerRegistrant {
        public MovementControllerRegistrant(MovementControllable controllable): base(controllable) {}

        public Action OnArrivedTarget = delegate { };
    } 
}
