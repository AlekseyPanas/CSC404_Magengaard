/*
Objects that provide functionality but no logic to trigger the functionality should implement this interface
*/

using System;
using Unity.Netcode;
using UnityEngine;

public abstract class AControllable<T> : NetworkBehaviour
{
    protected ControllerRegistrant _defaultController;
    protected ControllerRegistrant _currentController;
    int _currentPriority = int.MinValue;

    public event Action OnFree = delegate{};
    public AControllable(){
        _currentController = _defaultController;
    }

    /*

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
    Deregisters the given controller from the controllable.
    */
    private void DeRegisterController() {
        _currentController = _defaultController;
        _currentController.OnResume();
        _currentPriority = int.MinValue;
        OnFree();
    }

    /*
    Returns the current system. //todo
    */
    protected abstract T ReturnSelf();

    protected void OnControllerChange() {

    }

    public class ControllerRegistrant
    {
        private AControllable<T> controllable;
        public ControllerRegistrant (AControllable<T> controllable) {
            this.controllable = controllable;
        }
        public Action OnInterrupt = delegate {};
        public Action OnResume = delegate {};

        public T GetSystem(){
            if (controllable._currentController == this) {
                return controllable.ReturnSelf();
            } 
            return default;
        }

        public void DeRegister(){
            if(controllable._currentController == this){
                controllable.DeRegisterController();
            }
        }
    }
}
