using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/**
* Manages the entirety of spell casting:
*    - Stores a spell tree hierarchy
*    - Manages a gesture system by requesting recognition for specific spells in the tree
*    - Reacts to successful or failed gesture events modifying the tracked tree index accordingly
*    - Manages the caching system (not yet implemented)
*    - Detects a "tap" to finalize a successful spell sequence
*    - When a spell is cast, spawns an AimSystem and waits for aiming to finish via an event
*    - When a spell is done aiming, spawn it via ServerRPC
*/
public class SpellSystem: NetworkBehaviour {

    public static readonly float CHARGE_DEPLETION_TIME = 5;  // Time, in seconds, that it takes for one segment to deplete

    // Events that guide the gesture sequence visualization UI
    public static event Action GestureSequenceClearEvent = delegate {};  // Called when a player failed a gesture or didnt cast in time. The sequence starts from scratch
    public static event Action<Gesture, GestureBinNumbers, int> GestureSequenceAddEvent = delegate {};  // Append a new gesture to the sequence
    // Gesture: The gesture that was added, GestureBinNumbers: the accuracy bin of the gesture, int: how many charges were generated
    public static event Action<float> SetTimerBarPercentEvent = delegate {};  // Sets the fill percentage of the timer bar
    public static event Action<int, bool> SetSegmentFillEvent = delegate {};  // Sets the number of charges left of the current spell (usually used to deplete by 1). 
    // the bool is used to differentiate between depletion by casting (true) vs depletion by timer (false)

    private SpellTreeDS spellTreeRoot;

    // Spell casting trackers
    private List<int> spellPath;  // List of indexes into spell tree children to arrive at a particular spell node. Used to track currently casting spell
    private GestureBinNumbers _currSpellBinNum;  // The bin number accuracy of the most recently cast spell (i.e of the currently active spell)
    private float timestamp;  // Stamps time after a successful cast to track time between gestures
    private int _totalCharges;  // Number of total charges the currently active spell had in total at the start
    private int _currCharges;  // Number of charges the currently active spell has now
    //private List<int> spellBeingAimedPath = new();

    // Injectables
    [SerializeField] private ASpellTreeConfig _config;
    [SerializeField] private AGestureControllable _gestureSystem;
    private GestureSystemControllerRegistrant _gestRegistrant;
    private Transform ownPlayerTransform;

    private void Awake() {
        // Set player on spawn event
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += ply => {ownPlayerTransform = ply;};

        // Registers the spell system as the default gesture system controller
        _gestRegistrant = _gestureSystem.RegisterDefault();
        _gestRegistrant.OnResume += OnResume;
        _gestRegistrant.OnInterrupt += OnInterrupt;
    }
    
    /** Called when the gesture system resumes this default controller */
    private void OnResume() {
        _gestureSystem.GetSystem(_gestRegistrant).SetGesturesToRecognize(getCurrentChildren());
    }

    /** Called when another controller took over the gesture system. Clears the current spell path and destroys aim system in progress */
    private void OnInterrupt() {
        ClearSpellPath();
        //if (curAimSystem != null) { Destroy(curAimSystem.gameObject); }
    }

    private void Start() {
        timestamp = 0;

        // Initialize spellPath 
        spellPath = new List<int>();

        // Construct spell tree according to injected config
        spellTreeRoot = _config.buildTree();

        // Set gesture list of all children in currentNode and enable gesture system
        _gestureSystem.GetSystem(_gestRegistrant).SetGesturesToRecognize(getCurrentChildren());
        _gestureSystem.GetSystem(_gestRegistrant).enableGestureDrawing();

        // Subscribe to gesture success and fail events
        _gestRegistrant.GestureSuccessEvent += (int idx, GestureBinNumbers binNum) => {
            //Debug.Log("Gesure " + idx + " Successful!");
            AddToSpellPath(idx, binNum); // Add spell to the path
            _currSpellBinNum = binNum;
            _currCharges = getNodeFromSequence(spellPath).getValue().NumCharges;
            _totalCharges = _currCharges;
            //Debug.Log("\t\t\t\t\t\t\tAdded to path");

            // Create new aim system
            // GameObject aimSystemObject = Instantiate(getNodeFromSequence(spellPath).getValue().AimSystemPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            // curAimSystem = aimSystemObject.GetComponent<AAimSystem>();
            // curAimSystem.setPlayerTransform(ownPlayerTransform);  // Give player transform info
            // spellBeingAimedPath = new List<int>(spellPath);

            // Subscribe to finish aiming
            // curAimSystem.AimingFinishedEvent += (SpellParamsContainer spellParams) => {
            //     // Debug.Log("Played has aimed!\n\t\t\t\t\t\t\tCleared Spell Path; Spawned Spell;");
            //     SpawnSpellNormalServerRpc(spellBeingAimedPath.ToArray(), NetworkManager.Singleton.LocalClientId, spellParams);  // Spawn spell
            //     ClearSpellPath();  // Clear path
            //     Destroy(curAimSystem.gameObject);
            // };
        };
        // _gestRegistrant.beganDrawingEvent += () => {
        //     // Destroy aim system if new gesture started drawing
        //     if (curAimSystem != null) { /*Debug.Log("Player has begun a gesture!\n\t\t\t\t\t\t\tExisting AimSystem Destroyed");*/ Destroy(curAimSystem.gameObject); }
        // };
        _gestRegistrant.OnSwipeEvent += (Vector2 screenPoint) => {
            if (spellPath.Count > 0) {  // If a spell is active
                timestamp = Time.time;

                // Spawn the spell with the given bin number and swipe direction
                RaycastHit hit;
                bool isHit = Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0)), out hit);
                
                Vector3 direction;
                if (isHit) {
                    direction = hit.point - ownPlayerTransform.position;
                    direction = new Vector3(direction.x, 0, direction.z);
                } else {   
                    direction = new Vector3(0, 0, 1);
                }

                var container = new SpellParamsContainer();
                container.setVector3(0, direction.normalized);
                container.setFloat(0, (int)_currSpellBinNum);
                SpawnSpellNormalServerRpc(spellPath.ToArray(), NetworkManager.Singleton.LocalClientId, container);

                DecrementCharge(true);
            }
            
        };
        // _gestRegistrant.GestureBackfireEvent += idx => {
        //     // Spawn backfired spell and clear path
        //     AddToSpellPath(idx, fireEvent:false);
        //     SpawnSpellBackfireServerRpc(spellPath.ToArray(), NetworkManager.Singleton.LocalClientId);
        //     ClearSpellPath();
        // };
        _gestRegistrant.GestureFailEvent += () => {
            // Gesture failed, clear the path
            // Debug.Log("Player has failed the gesture!; Spell Path cleared");
            ClearSpellPath();
        };
    }

    private void Update() {
        // Clear path if time ran out
        if (spellPath.Count > 0) {
            float percentDepleted = (Time.time - timestamp) / CHARGE_DEPLETION_TIME;
            if (percentDepleted >= 1) {
                DecrementCharge(false);
                timestamp = Time.time;
            } else {
                SetTimerBarPercentEvent(1 - percentDepleted);
            }
        } 
    }

    /** 
    * Removes 1 charge, checks if the number of charges left is 0, and if so, executes appropriate cleanup.
    * Also fires UI event to deplete a charge
    */
    private void DecrementCharge(bool isDueToSpellCast) {
        _currCharges--;
        SetSegmentFillEvent(_currCharges, isDueToSpellCast);
        if (_currCharges == 0) {
            ClearSpellPath();
        }
    }

    /** Clears the spell path, fires appropriate event, resets timestamp, updates the gestures to recognize, and sends time bar set event */
    private void ClearSpellPath() {
        spellPath.Clear();
        GestureSequenceClearEvent();
        timestamp = Time.time;
        SetTimerBarPercentEvent(1);
        _gestureSystem.GetSystem(_gestRegistrant).SetGesturesToRecognize(getCurrentChildren());
    }

    /** Adds a single index to spell path, fires appropriate event, resets timestamp, updates the gestures to recognize, and sends time bar set event */
    private void AddToSpellPath(int idx, GestureBinNumbers binNum, bool fireEvent = true) {
        spellPath.Add(idx);
        if (fireEvent) {GestureSequenceAddEvent(getNodeFromSequence(spellPath).getValue().Gesture, binNum, getNodeFromSequence(spellPath).getValue().NumCharges);}
        timestamp = Time.time;
        SetTimerBarPercentEvent(1);
        _gestureSystem.GetSystem(_gestRegistrant).SetGesturesToRecognize(getCurrentChildren());
    }

    /** Sets spell path to given sequence, fires appropriate event, resets timestamp, and updates the gestures to recognize */
    // private void SetSpellPath(List<int> idxSeq) {
    //     spellPath = idxSeq;
    //     timestamp = Time.time;
    //     _gestureSystem.GetSystem(_gestRegistrant).SetGesturesToRecognize(getCurrentChildren());
    // }

    /** 
    * Return the node at the given seq, where seq is a sequence of indexes into children of the tree starting from the root
    */
    private SpellTreeDS getNodeFromSequence(List<int> seq) {
        SpellTreeDS cur = spellTreeRoot;
        foreach (int i in seq) {cur = cur.getChild(i);}
        return cur;
    }

    /** Get a list of gestures corresponding to child spells of the node at the current spellPath */
    private List<Gesture> getCurrentChildren() {
        List<Gesture> gestList = new List<Gesture>();
        foreach (SpellTreeDS s in getNodeFromSequence(spellPath).getChildren()) {
            gestList.Add(s.getValue().Gesture);
        }
        return gestList;
    }

    /**
    * :param treeIndexSequence: index into spellTree to access the spellDS that was cast
    * :param playerId: Netcode id of the client
    * :param spellParams: The bulky data class containing necessary data for corresponding spell
    */
    [ServerRpc(RequireOwnership = false)]
    private void SpawnSpellNormalServerRpc(int[] treeIndexSequence, ulong playerId, SpellParamsContainer spellParams) {
        SpawnSpellServerSide(treeIndexSequence, playerId, spellParams);
    }

    /**
    * :param treeIndexSequence: index into spellTree to access the spellDS that was cast
    * :param playerId: Netcode id of the client
    */
    [ServerRpc(RequireOwnership = false)]
    private void SpawnSpellBackfireServerRpc(int[] treeIndexSequence, ulong playerId) {
        SpawnSpellServerSide(treeIndexSequence, playerId, null);
    }

    /** Called by the two server RPCs, should NOT be called by anything else */
    private void SpawnSpellServerSide(int[] treeIndexSequence, ulong playerId, SpellParamsContainer spellParams) {
        if (IsServer) {
            // Find spell via tree index sequence
            SpellTreeDS cur = getNodeFromSequence(treeIndexSequence.ToList());

            // Instantiate and spawn on network
            GameObject spell = Instantiate(cur.getValue().SpellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spell.GetComponent<ISpell>().setPlayerId(playerId);
            
            if (spellParams != null) { spell.GetComponent<ISpell>().preInit(spellParams); }
            else { spell.GetComponent<ISpell>().preInitBackfire(); }
            
            // Spawn to all clients
            spell.GetComponent<NetworkObject>().Spawn();

            spell.GetComponent<ISpell>().postInit();
        }
    }
}
