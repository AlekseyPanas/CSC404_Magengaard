/*
Objects that provide functionality but no logic to trigger the functionality should implement this interface. For example, the movement controllable would
be a system that exposes functionality to move the player but does not include logic for it. The User Input Controller class would then register itself as
a controller of that movement controllable and implement logic to move the player using user input
*/

using System;
using Unity.Netcode;
using UnityEngine;

public abstract class AControllable<T> : NetworkBehaviour
{
    protected ControllerRegistrant _defaultController;  //
    protected ControllerRegistrant _currentController;
    int _currentPriority = int.MinValue;

    public event Action OnFree = delegate{};
    public AControllable(){
        _currentController = _defaultController;
    }

    /** 
    * Register a default controller and return the registration data object. If a previous default was already registered, return null 
    */
    public ControllerRegistrant RegisterDefault() {
        if (_defaultController == null) {
            _defaultController = new ControllerRegistrant(this);
            if (_currentController == null) {
                _currentController = _defaultController;
            }
            return _defaultController;
        } 
        return null;
    }

    /*
    * Try to register a new controller with this controllable. If another controller is registered with a higher or equal priority to the one provided, return null.
    * Otherwise create and return a new registration object.
    */
    public ControllerRegistrant RegisterController(int priority) {
        if (priority > _currentPriority) {
            _currentController.OnInterrupt();
            _currentController = new ControllerRegistrant(this);
            _currentPriority = priority;
            return _currentController;
        }
        return null;
    }

    /*
    Deregisters the current controller and fires appropriate events
    */
    private void DeRegisterController() {
        _currentController = _defaultController;
        _currentController.OnResume();
        _currentPriority = int.MinValue;
        OnFree();
    }

    /*
    Implementing classes should populate this with 'return this'. It should return the type of the inheriting class
    */
    protected abstract T ReturnSelf();

    protected void OnControllerChange() {}

    /**
    * Registration data returned to a controller that registers itself with the controllable. This registration data is the means by which
    * the controller interacts with the controllable
    */
    public class ControllerRegistrant {
        private AControllable<T> controllable;
        internal ControllerRegistrant (AControllable<T> controllable) { this.controllable = controllable; }

        public Action OnInterrupt = delegate {};  /** Called when this controller was the current controller of this controllable but replaced by another controller with a higher priority */
        public Action OnResume = delegate {};  /** Called only for the default controller when that controller was reinstated due to the former controller deregistering */

        /** Return the underlying controllable system. Return the default/null type if this registrant object is not the current controller for the controllable */
        public T GetSystem(){
            if (controllable._currentController == this) {
                return controllable.ReturnSelf();
            } 
            return default;
        }

        /** Remove this controller as the current controller and resume the default controller. After deregistering, this class can be discarded since a new
        registration generates a new registrant object. If this is the default controller, deregistering does nothing (i.e the default will always be the default) */
        public void DeRegister(){
            if(controllable._currentController == this){
                controllable.DeRegisterController();
            }
        }
    }
}
