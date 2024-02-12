public delegate void FinishedTask();

/** Inherit this interface and regsiter with the MainMenuLogic singleton if you want the menu to wait for this object
to do something before proceeding with the scene switch after a key is pressed (e.g the camera "lays down" before the screen fades) */
public interface IWaitBeforeFade {
    public event FinishedTask FinishedTaskEvent;
}
