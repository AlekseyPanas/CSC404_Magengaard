using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireIceFightController : MonoBehaviour
{
    Camera cam;
    [SerializeField] EnemySpawner fireSpawner;
    [SerializeField] EnemySpawner iceSpawner;
    [SerializeField] FireTerminal fireTerminal;
    [SerializeField] IceTerminal iceTerminal;
    [SerializeField] PlayerDetector pd;
    [SerializeField] int numEnemiesSpawnedPerSpawner;
    bool iceTerminalDormant;
    bool fireTerminalDormant;

    void Start(){
        fireTerminal.ToggleDormant(false);
        iceTerminal.ToggleDormant(false);
        pd.OnPlayerEnter += StartSequence;
    }

    /*
    Subscribe these functions to the spell page pickup events
    */
    void SetFireTerminalInactive(){ 
        fireTerminal.ToggleDormant(false);
        fireTerminalDormant = false;
    }

    void SetIceTerminalInactive(){
        iceTerminal.ToggleDormant(false);
        iceTerminalDormant = false;
    }

    void StartSequence(GameObject player){
        if(fireTerminalDormant || iceTerminalDormant) return;

        fireTerminal.ToggleDormant(true);
        iceTerminal.ToggleDormant(true);
        fireSpawner.SpawnNumEnemies(numEnemiesSpawnedPerSpawner);
        iceSpawner.SpawnNumEnemies(numEnemiesSpawnedPerSpawner);

    }
}
