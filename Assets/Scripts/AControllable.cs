/*
Objects that provide functionality but no logic to trigger the functionality should implement this interface. For example, the movement controllable would
be a system that exposes functionality to move the player but does not include logic for it. The User Input Controller class would then register itself as
a controller of that movement controllable and implement logic to move the player using user input
*/

using System;
using Unity.Netcode;
using UnityEngine;

public abstract class AControllable<T, R> : NetworkBehaviour where R: ControllerRegistrant, new()
{
    protected R _defaultController = null;  //
    protected R _currentController;
    protected int _currentPriority = int.MinValue;

    public event Action OnFree = delegate{};
    public AControllable(){
        _currentController = _defaultController;
    }

    /** 
    * Register a default controller and return the registration data object. If a previous default was already registered, return null 
    */
    public virtual R RegisterDefault() {
        if (_defaultController == null) {
            _defaultController = new R();
            if (_currentController == null) {
                _currentController = _defaultController;
                OnControllerChange();
            }
            return _defaultController;
        } 
        return null;
    }

    /*
    * Try to register a new controller with this controllable. If another controller is registered with a higher or equal priority to the one provided, return null.
    * Otherwise create and return a new registration object.
    */
    public virtual R RegisterController(int priority) {
        if (priority >= _currentPriority) {
            _currentController.OnInterrupt();
            _currentController = new R();
            _currentPriority = priority;
            OnControllerChange();
            return _currentController;
        }
        return null;
    }

    /** Remove a controller as the current controller and resume the default controller. After deregistering a ControllerRegistrant, the registrant can be discarded since a new
    registration generates a new registrant object. If the default controller is passed, deregistering does nothing (i.e the default will always be the default) */
    public virtual void DeRegisterController(R controller) {
        if(_currentController == controller) {
            _currentController = _defaultController;
            _currentController.OnResume();
            _currentPriority = int.MinValue;
            OnFree();
            OnControllerChange();
        }
    }

    /** Return the underlying controllable system. Return the default/null type if this registrant object is not the current controller for the controllable */
    public T GetSystem(R controller){
        if (_currentController == controller) {
            return ReturnSelf();
        } 
        return default;
    }

    /*
    Implementing classes should populate this with 'return this'. It should return the type of the inheriting class
    */
    protected abstract T ReturnSelf();

    protected virtual void OnControllerChange() {}
}


/**
* Registration data returned to a controller that registers itself with the controllable. This registration data is the means
* through which a controller receives events from a controllable. It is also the "ticket" of verification needed to call
* controllable methods
*/
public class ControllerRegistrant {
    public Action OnInterrupt = delegate {};  /** Called when this controller was the current controller of this controllable but replaced by another controller with a higher priority */
    public Action OnResume = delegate {};  /** Called only for the default controller when that controller was reinstated due to the former controller deregistering */
}
