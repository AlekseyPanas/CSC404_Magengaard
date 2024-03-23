using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using APlayerHeathControllable = AControllable<PlayerHealthControllable, ControllerRegistrant>;
using UnityEngine;
using System.Collections;

public class FireIceFightController : MonoBehaviour
{
    [SerializeField] EnemySpawner fireSpawner;
    [SerializeField] EnemySpawner iceSpawner;
    [SerializeField] FireTerminal fireTerminal;
    [SerializeField] IceTerminal iceTerminal;
    [SerializeField] int numEnemiesSpawnedPerSpawner;
    [SerializeField] Transform fireIceDoorCamera;
    [SerializeField] Transform fireSpritesCamera;
    [SerializeField] Transform iceSpritesCamera;
    [SerializeField] int _dormantTerminalCount = 2;
    [SerializeField] GameObject respawnPoint;
    [SerializeField] ParticleSystem fireTrail;
    [SerializeField] ParticleSystem iceTrail;
    [SerializeField] GameObject fireGodRay;
    [SerializeField] GameObject iceGodRay;
    GameObject player;
    bool fireEnemiesCleared = false;
    bool iceEnemiesCleared = false;
    APlayerHeathControllable healthSystem;
    AMovementControllable movementSystem;
    AGestureControllable gestureSystem;
    ACameraControllable cameraSystem;
    ControllerRegistrant healthSystemRegistrant;
    MovementControllerRegistrant movementSystemRegistrant;
    GestureSystemControllerRegistrant gestureSystemRegistrant;
    ControllerRegistrant cameraSystemRegistrant;



    void Start(){
        player = GameObject.FindWithTag("Player");
        healthSystem = player.GetComponent<APlayerHeathControllable>();
        movementSystem = player.GetComponent<AMovementControllable>();
        cameraSystem = FindFirstObjectByType<ACameraControllable>().GetComponent<ACameraControllable>();
        gestureSystem = GestureSystem.ControllableInstance;
        fireSpawner.OnEnemiesCleared += SetFireEnemiesCleared;
        iceSpawner.OnEnemiesCleared += SetIceEnemiesCleared;
        respawnPoint.SetActive(false);
        fireTerminal.GetComponent<ITerminal>().ToggleDormant(true);
        iceTerminal.GetComponent<ITerminal>().ToggleDormant(true);

        fireTerminal.GetComponent<ITerminal>().OnCrystalPlaced += () => {
            _dormantTerminalCount--;
            fireGodRay.SetActive(false);
            fireTrail.Stop();
            StartSequence();
        };
        iceTerminal.GetComponent<ITerminal>().OnCrystalPlaced += () => {
            _dormantTerminalCount--;
            iceGodRay.SetActive(false);
            iceTrail.Stop();
            StartSequence();
        };
    }

    void SetFireEnemiesCleared(){
        fireEnemiesCleared = true;
        if(iceEnemiesCleared){
            SetTerminalsInactive();
        }
    }

    void SetIceEnemiesCleared(){
        iceEnemiesCleared = true;
        if(fireEnemiesCleared){
            SetTerminalsInactive();
        }
    }

    void SetTerminalsInactive(){
        if(!TryRegister()){ 
            Debug.Log("register failed");
            return;
        }
        StartCoroutine(ShowTerminalsInactive());
    }

    IEnumerator ShowTerminalsInactive(){
        ICameraFollow currFollow = cameraSystem.GetSystem(cameraSystemRegistrant).GetCurrentFollow(cameraSystemRegistrant);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            new CameraFollowFixed(fireIceDoorCamera.position, fireIceDoorCamera.forward, 1f));
        yield return new WaitForSeconds(1f);
        fireTerminal.GetComponent<ITerminal>().ToggleDormant(false);
        iceTerminal.GetComponent<ITerminal>().ToggleDormant(false);
        yield return new WaitForSeconds(2f);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            currFollow);
        yield return new WaitForSeconds(1f);
        DeRegisterAll();
    }

    /*
    Subscribe these functions to the spell page pickup events
    */
    // void SetFireTerminalInactive(){ 
    //     fireTerminal.ToggleDormant(false);
    //     fireTerminalDormant = false;
    // }

    // void SetIceTerminalInactive(){
    //     iceTerminal.ToggleDormant(false);
    //     iceTerminalDormant = false;
    // }

    void StartSequence(){
        if(_dormantTerminalCount > 0) return;
        fireSpawner.SpawnNumEnemies(numEnemiesSpawnedPerSpawner);
        iceSpawner.SpawnNumEnemies(numEnemiesSpawnedPerSpawner);
        
        if(!TryRegister()){ 
            Debug.Log("register failed");
            return;
        }

        StartCoroutine(ShowDoorAndEnemies());
    }

    IEnumerator ShowDoorAndEnemies(){
        ICameraFollow currFollow = cameraSystem.GetSystem(cameraSystemRegistrant).GetCurrentFollow(cameraSystemRegistrant);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            new CameraFollowFixed(fireIceDoorCamera.position, fireIceDoorCamera.forward, 1f));
        //yield return new WaitForSeconds(1f);
        // fireTerminal.GetComponent<ITerminal>().DisableTerminal();
        // iceTerminal.GetComponent<ITerminal>().DisableTerminal();
        yield return new WaitForSeconds(2f);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            new CameraFollowFixed(fireSpritesCamera.position, fireSpritesCamera.forward, 2f));
        yield return new WaitForSeconds(4f);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            new CameraFollowFixed(iceSpritesCamera.position, iceSpritesCamera.forward, 2f));
        yield return new WaitForSeconds(4f);
        cameraSystem.GetSystem(cameraSystemRegistrant).SwitchFollow(cameraSystemRegistrant, 
            currFollow);
        yield return new WaitForSeconds(1f);
        fireSpawner.EnableEnemiesAI(player);
        iceSpawner.EnableEnemiesAI(player);
        DeRegisterAll();
    }

    bool TryRegister(){
        healthSystemRegistrant = healthSystem.RegisterController((int)HealthControllablePriorities.CUTSCENE);
        movementSystemRegistrant = movementSystem.RegisterController((int)MovementControllablePriorities.CUTSCENE);
        gestureSystemRegistrant = gestureSystem.RegisterController((int)GestureControllablePriorities.CUTSCENE);
        cameraSystemRegistrant = cameraSystem.RegisterController((int)CameraControllablePriorities.CUTSCENE);

        if (healthSystemRegistrant == null || movementSystemRegistrant == null || gestureSystemRegistrant == null || cameraSystemRegistrant == null){
            DeRegisterAll();
            return false;
        }
        
        gestureSystem.GetSystem(gestureSystemRegistrant).disableGestureDrawing();

        return true;
    }
    void DeRegisterAll() {
        gestureSystem.GetSystem(gestureSystemRegistrant).enableGestureDrawing();

        healthSystem.DeRegisterController(healthSystemRegistrant);
        movementSystem.DeRegisterController(movementSystemRegistrant);
        gestureSystem.DeRegisterController(gestureSystemRegistrant);
        cameraSystem.DeRegisterController(cameraSystemRegistrant);
    }
}
