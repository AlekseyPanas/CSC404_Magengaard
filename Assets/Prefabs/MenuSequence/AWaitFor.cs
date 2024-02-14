using UnityEngine;

public delegate void FinishedTask();

/** Object will send an event when they finish a task. An object can hold a bunch of IWaitFor instances and wait for them all to finish */
public abstract class AWaitFor: MonoBehaviour {
    public event FinishedTask FinishedTaskEvent = delegate {}; protected void invokeFinishedTask() { FinishedTaskEvent(); }
}
