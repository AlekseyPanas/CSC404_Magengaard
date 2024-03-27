using UnityEngine;

using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using APlayerHeathControllable = AControllable<PlayerHealthControllable, ControllerRegistrant>;

public class Cutscene : MonoBehaviour
{
    protected APlayerHeathControllable _healthSystem;
    protected AMovementControllable _movementSystem;
    protected AGestureControllable _gestureSystem;
    protected ACameraControllable _cameraSystem;
    
    protected ControllerRegistrant _healthSystemRegistrant;
    protected MovementControllerRegistrant _movementSystemRegistrant;
    protected GestureSystemControllerRegistrant _gestureSystemRegistrant;
    protected ControllerRegistrant _cameraSystemRegistrant;
    
    public CameraPosition ToPosition(Transform t)
    {
        return new CameraPosition
        {
            Position = t.position,
            Forward = t.forward,
        };
    }

    public void LoadWithPlayer(Transform player)
    {
        Debug.Log("On Start!");
        
        _healthSystem = player.GetComponent<APlayerHeathControllable>();
        _movementSystem = player.GetComponent<AMovementControllable>();
        _cameraSystem = FindFirstObjectByType<ACameraControllable>().GetComponent<ACameraControllable>();
        _gestureSystem = GestureSystem.ControllableInstance;
    }
    
    public bool TryRegister(){
        _healthSystemRegistrant = _healthSystem.RegisterController((int)HealthControllablePriorities.CUTSCENE);
        _movementSystemRegistrant = _movementSystem.RegisterController((int)MovementControllablePriorities.CUTSCENE);
        _gestureSystemRegistrant = _gestureSystem.RegisterController((int)GestureControllablePriorities.CUTSCENE);
        _cameraSystemRegistrant = _cameraSystem.RegisterController((int)CameraControllablePriorities.CUTSCENE);

        if (_healthSystemRegistrant == null || _movementSystemRegistrant == null || _gestureSystemRegistrant == null || _cameraSystemRegistrant == null){
            DeRegisterAll();
            return false;
        }
        
        _gestureSystem.GetSystem(_gestureSystemRegistrant).disableGestureDrawing();

        return true;
    }
    public void DeRegisterAll() {
        _gestureSystem.GetSystem(_gestureSystemRegistrant).enableGestureDrawing();

        _healthSystem.DeRegisterController(_healthSystemRegistrant);
        _movementSystem.DeRegisterController(_movementSystemRegistrant);
        _gestureSystem.DeRegisterController(_gestureSystemRegistrant);
        _cameraSystem.DeRegisterController(_cameraSystemRegistrant);
    }
}