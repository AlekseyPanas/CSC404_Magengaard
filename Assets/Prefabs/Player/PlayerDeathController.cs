using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using APlayerHeathControllable = AControllable<PlayerHealthControllable, ControllerRegistrant>;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathController : NetworkBehaviour, IKillable
{
    [SerializeField] GameObject respawnVFX;
    [SerializeField] GameObject playerModel;
    [SerializeField] float respawnStartDelay = 3f;
    [SerializeField] float respawnEventDelay = 3f;
    [SerializeField] float showPlayerDelay = 1f;
    [SerializeField] float showRespawnVFXDelay = 1f;
    public event Action<GameObject> OnDeath;
    public static event Action OnRespawn = delegate {};
    public static event Action OnRespawnFinished = delegate {};

    APlayerHeathControllable healthSystem;
    AMovementControllable movementSystem;
    AGestureControllable gestureSystem;
    ACameraControllable cameraSystem;
    
    ControllerRegistrant healthSystemRegistrant;
    MovementControllerRegistrant movementSystemRegistrant;
    GestureSystemControllerRegistrant gestureSystemRegistrant;
    ControllerRegistrant cameraSystemRegistrant;
    Vector3 _damageDir;
    GameObject respawnPoint;
    public int _sequenceCounter = 0;

    void Start() { 
        healthSystem = GetComponent<APlayerHeathControllable>();
        movementSystem = GetComponent<AMovementControllable>();
        cameraSystem = FindFirstObjectByType<ACameraControllable>().GetComponent<ACameraControllable>();
        gestureSystem = GestureSystem.ControllableInstance;
        PlayerHealthControllable.OnHealthPercentChange += StartRespawnSequence;
    }

    /*
    Called when the player dies. Disables the player's collider and plays the death animation corresponding to which direction
    the player took damage from.
    The collider is disabled and reenabled after to allow the camera to reset to its intended region.
    Tries to register itself to the Movement system, gesture system, and camera system.
    Disables player movement and gesture system, and takes over camera system.
    */
    void Die(){
        if (!TryRegister()) return;

        OnDeath(gameObject); //destroys existing aim systems, deagros enemies, and hides player ui and starts the black screen
        _damageDir = new Vector3(_damageDir.x, 0, _damageDir.z);
        float[] angles = {  Vector3.Angle(_damageDir, transform.right),    // fall left death3
                            Vector3.Angle(_damageDir, -transform.right),    // fall right death4
                            Vector3.Angle(_damageDir, -transform.forward),    // fall forward death2
                            Vector3.Angle(_damageDir, transform.forward) };  // fall backwards death1
        int i = Array.IndexOf(angles, angles.Min());

        var s = movementSystem.GetSystem(movementSystemRegistrant);
        s.GetAnimator().SetInteger(s.GetAnimStateHash(), (int)AnimationStates.DEATH_LEFT + i);
        s.GetAnimator().SetTrigger(s.GetAnimTransitionTriggerStateHash());
        GetComponent<Collider>().enabled = false;
    }

    /*
    Attempts to register to the health, gesture, and movement systems.
    If any of the three are currently registered by a controller with a higher priority, deregister from all of them
    */
    bool TryRegister(){
        healthSystemRegistrant = healthSystem.RegisterController((int)HealthControllablePriorities.DEATH);
        movementSystemRegistrant = movementSystem.RegisterController((int)MovementControllablePriorities.DEATH); //registering will stop user input.
        gestureSystemRegistrant = gestureSystem.RegisterController((int)GestureControllablePriorities.DEATH);
        cameraSystemRegistrant = cameraSystem.RegisterController((int)CameraControllablePriorities.DEATH);

        if (healthSystemRegistrant == null || movementSystemRegistrant == null || gestureSystemRegistrant == null || cameraSystemRegistrant == null){
            DeRegisterAll();
            return false;
        }

        healthSystemRegistrant.OnInterrupt += OnInterrupt;
        movementSystemRegistrant.OnInterrupt += OnInterrupt;
        gestureSystemRegistrant.OnInterrupt += OnInterrupt;
        cameraSystemRegistrant.OnInterrupt += OnInterrupt;

        return true;
    }

    void DeRegisterAll() {
        healthSystemRegistrant.OnInterrupt -= OnInterrupt;
        movementSystemRegistrant.OnInterrupt -= OnInterrupt;
        gestureSystemRegistrant.OnInterrupt -= OnInterrupt;
        cameraSystemRegistrant.OnInterrupt -= OnInterrupt;

        healthSystem.DeRegisterController(healthSystemRegistrant);
        movementSystem.DeRegisterController(movementSystemRegistrant);
        gestureSystem.DeRegisterController(gestureSystemRegistrant);
        cameraSystem.DeRegisterController(cameraSystemRegistrant);
    }

    /*
    Hides the player model.
    Find the respawn point with the highest counter and moves the player to the respawn point.
    Overides current camera region and moves the camera to the respawn point.
    */
    void StartRespawn(){
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        int num = -1;
        respawnPoint = respawnPoints[0];
        foreach (GameObject r in respawnPoints) {
            RespawnPoint rp = r.GetComponent<RespawnPoint>();
            if (rp.isActive) {
                int currNum = rp.number;
                if (currNum > num) {
                    num = currNum;
                    respawnPoint = r;
                }
            }
        }
        playerModel.SetActive(false);
        GetComponent<CharacterController>().enabled = false;  // TODO: Probably should add a method to the movement system to do this through the controllable architecture
        transform.position = respawnPoint.transform.position;
        GetComponent<CharacterController>().enabled = true;
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, new CameraFollowFixed(transform.position, transform.forward, 0.1f));
    }

    /*
    Resets the camera override and reenables the collider to allow the current camera region to take over.
    */
    void EnablePlayerCollider(){
        cameraSystem.DeRegisterController(cameraSystemRegistrant);
        GetComponent<Collider>().enabled = true;
    }

    void Respawn(){
        OnRespawn(); //Player UI resets HP bar to full and fades the black cover to transparent.
    }
    void ShowRespawnVFX(){
        Instantiate(respawnVFX, respawnPoint.transform.position + new Vector3(0,-3f,0), Quaternion.Euler(new Vector3(-90,0,0)));
    }

    /*
    Show the player after it has been moved to the respawn location. Reset the animation and reset the player's HP.
    */
    void ShowPlayer(){
        OnRespawnFinished(); // currently not assigned to anything
        playerModel.SetActive(true);
        var m = movementSystem.GetSystem(movementSystemRegistrant);
        m.GetAnimator().SetInteger(m.GetAnimStateHash(), (int)AnimationStates.IDLE);
        gestureSystem.GetSystem(gestureSystemRegistrant).enableGestureDrawing();
        healthSystem.GetSystem(healthSystemRegistrant).ResetHP();
        DeRegisterAll();
    }

    void StartRespawnSequence(float percent, Vector3 dir){
        if (percent > 0) return;
        _damageDir = dir;
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence(){
        if (_sequenceCounter < 1) {
            Die();
            _sequenceCounter = 1;
            yield return new WaitForSeconds(respawnStartDelay);
        }
        if (_sequenceCounter < 2) {
            StartRespawn();
            _sequenceCounter = 2;
            yield return new WaitForSeconds(0.1f);
        }
        if (_sequenceCounter < 3) {
            EnablePlayerCollider();
            _sequenceCounter = 3;
            yield return new WaitForSeconds(respawnEventDelay);
        }
        if (_sequenceCounter < 4) {
            Respawn();
            _sequenceCounter = 4;
            yield return new WaitForSeconds(showRespawnVFXDelay);
        }
        if (_sequenceCounter < 5) {
            ShowRespawnVFX();
            _sequenceCounter = 5;
            yield return new WaitForSeconds(showPlayerDelay);
        }
        if (_sequenceCounter < 6) {
            _sequenceCounter = 6;
            ShowPlayer();
        }
        _sequenceCounter = 0;
    }

    void OnInterrupt(){
        StopCoroutine(nameof(RespawnSequence));
    }
}
