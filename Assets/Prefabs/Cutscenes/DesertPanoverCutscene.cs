using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using APlayerHeathControllable = AControllable<PlayerHealthControllable, ControllerRegistrant>;

public class DesertPanoverCutscene : MonoBehaviour
{
    public Transform panover1Start;
    public Transform panover1End;
    public Transform panover2Start;
    public Transform panover2End;
    public Transform panover3Start;
    public Transform panover3End;

    public bool startOnLoad;
    
    private APlayerHeathControllable _healthSystem;
    private AMovementControllable _movementSystem;
    private AGestureControllable _gestureSystem;
    private ACameraControllable _cameraSystem;
    
    private ControllerRegistrant _healthSystemRegistrant;
    private MovementControllerRegistrant _movementSystemRegistrant;
    private GestureSystemControllerRegistrant _gestureSystemRegistrant;
    private ControllerRegistrant _cameraSystemRegistrant;

    public FadeToBlackPanel panel;
    public GameObject canvas;

    private CameraPosition ToPosition(Transform t)
    {
        return new CameraPosition
        {
            Position = t.position,
            Forward = t.forward,
        };
    }

    private IEnumerator CutsceneEvents()
    {
        yield return 0; // allow time for the rest of the game to get working
        
        var player = GameObject.FindWithTag("Player");
        
        LoadWithPlayer(player.transform);

        if (!TryRegister())
        {
            yield break;
        }
        
        var cameraManager = _cameraSystem.GetSystem(_cameraSystemRegistrant);

        if (cameraManager == null)
        {
            yield break;
        }

        var initialFollow = cameraManager.GetCurrentFollow(_cameraSystemRegistrant);

        const float fadeTime = 0.5f;
        
        canvas.SetActive(true);
        
        cameraManager.JumpTo(ToPosition(panover1Start));
        cameraManager.SwitchFollow(_cameraSystemRegistrant,
            new CameraFollowFixed(panover1End.position, panover1End.forward, 5.0f));

        yield return new WaitForSeconds(3.0f);
        
        panel.startFadingToBlack(fadeTime);

        yield return new WaitForSeconds(fadeTime);
        
        cameraManager.JumpTo(ToPosition(panover2Start));
        cameraManager.SwitchFollow(_cameraSystemRegistrant,
            new CameraFollowFixed(panover2End.position, panover2End.forward, 5.0f));
        
        panel.startFadingToTransparent(fadeTime);
        
        yield return new WaitForSeconds(3.0f);
        
        panel.startFadingToBlack(fadeTime);
        
        yield return new WaitForSeconds(fadeTime);
        
        cameraManager.JumpTo(ToPosition(panover3Start));
        cameraManager.SwitchFollow(_cameraSystemRegistrant,
            new CameraFollowFixed(panover3End.position, panover3End.forward, 2.5f));
        
        panel.startFadingToTransparent(fadeTime);
        
        panel.startFadingToBlack(fadeTime);
        yield return new WaitForSeconds(fadeTime);
        
        cameraManager.SwitchFollow(_cameraSystemRegistrant, initialFollow);
        
        panel.startFadingToTransparent(fadeTime);
        
        canvas.SetActive(false);
        
        // End
        
        DeRegisterAll();
    }
    
    public void StartCutscene()
    {
        StartCoroutine(CutsceneEvents());
    }

    private void LoadWithPlayer(Transform player)
    {
        Debug.Log("On Start!");
        
        _healthSystem = player.GetComponent<APlayerHeathControllable>();
        _movementSystem = player.GetComponent<AMovementControllable>();
        _cameraSystem = FindFirstObjectByType<ACameraControllable>().GetComponent<ACameraControllable>();
        _gestureSystem = GestureSystem.ControllableInstance;
    }
    
    private void Start()
    {
        if (startOnLoad)
        {
            StartCutscene();
        }
        // Own Player Spawned Event seems bugged right now.
        // Flat out doesn't fire. Am I too late?
        // I don't see the code that fires this event on the player script anymore.
        // So leaving it alone in the interest of time.
        // PlayerSpawnedEvent.OwnPlayerSpawnedEvent += StartWithPlayer;
    }

    bool TryRegister(){
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
    void DeRegisterAll() {
        _gestureSystem.GetSystem(_gestureSystemRegistrant).enableGestureDrawing();

        _healthSystem.DeRegisterController(_healthSystemRegistrant);
        _movementSystem.DeRegisterController(_movementSystemRegistrant);
        _gestureSystem.DeRegisterController(_gestureSystemRegistrant);
        _cameraSystem.DeRegisterController(_cameraSystemRegistrant);
    }
}
