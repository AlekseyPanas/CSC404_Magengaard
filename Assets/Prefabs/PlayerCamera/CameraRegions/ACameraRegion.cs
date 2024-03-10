using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/** 
* Takes care of boiler plate of detecting a player within the region, registering with camera, waiting for camera freedom if busy, and calling
* an abstract method when control of camera system is acquired. Implementing regions should implement OnTriggerRegion and use that to call
* SwitchFollow on the underlying system
*/
public abstract class ACameraRegion: MonoBehaviour {

    private ACameraControllable _cam;
    private Collider _waitingPlayer = null;

    /** 
    * Acquire the camera controllable
    */
    void Awake() { 
        _cam = Camera.main.GetComponent<ACameraControllable>(); 
        _cam.OnFree += OnCamFree;
    }

    /** 
    * If collided with player, try to register and trigger the region 
    */
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag != "Player") { return; }  // TODO: Very likely will need to check if this is your own player in multiplayer

        TryRegisterAndTrigger(other);
    }

    /** 
    * Tries registering controller and subsequently triggering camera. If registration fails, queue up the collider.
    */
    bool TryRegisterAndTrigger(Collider other) {
        // Try to register as a controller
        var controller = _cam.RegisterController((int)CameraControllablePriorities.REGION);
        if (controller == null) { 
            _waitingPlayer = other;
            return false; 
        }
        OnTriggeredRegion(_cam, controller, other);
        return true;
    }

    /** 
    * Removes queued collider if it exited
    */
    void OnTriggerExit(Collider other) {
        if (other == _waitingPlayer) {
            _waitingPlayer = null;
        }
    }

    /** 
    * If camera freed up and there is a queued collider, attempt to register it again
    */
    void OnCamFree() {
        if (_waitingPlayer != null) {
            if (TryRegisterAndTrigger(_waitingPlayer)) _waitingPlayer = null;
        }
    }

    /** Called when a player entered the region AND control of the camera was established */
    protected abstract void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player);
}
