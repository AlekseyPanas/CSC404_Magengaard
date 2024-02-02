using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public delegate void GestureSequenceClear();
public delegate void GestureSequenceAdd(Gesture g);
public delegate void GestureSequenceSet(List<Gesture> gs);

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

    public static readonly float TIME_BETWEEN_GESTURES = 2;  // Time, in seconds, that a player has to cast the next gesture or tap to finish before the sequence clears

    // Events that guide the gesture sequence visualization UI
    public static event GestureSequenceClear GestureSequenceClearEvent;  // Called when a player failed a gesture or didnt cast in time. The sequence starts from scratch
    public static event GestureSequenceAdd GestureSequenceAdd;  // Append a new gesture to the sequence
    public static event GestureSequenceSet GestureSequenceSet;  // Set the current sequence clearing the previous one implicitly. Usually happens when a scroll is activated
    
    private SpellTreeDS spellTreeRoot;
    private List<int> spellPath;  // List of indexes into spell tree children to arrive at a particular spell node. Used to track currently casting spell
    
    private float timestamp;  // Stamps time after a successful cast to track time between gestures

    [SerializeField] private ASpellTreeConfig config;
    [SerializeField] private AGestureSystem gestureSystem;
    private Transform ownPlayerTransform;
    
    private void Start() {
        timestamp = 0;

        // Set player on spawn event
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform ply) => {ownPlayerTransform = ply;};

        // Construct spell tree according to injected config
        spellTreeRoot = config.buildTree();

        // Set gesture list of all children in currentNode and enable gesture system
        gestureSystem.setGesturesToRecognize(getCurrentChildren());
        gestureSystem.enableGestureDrawing();

        // Subscribe to gesture success and fail events
        gestureSystem.GestureSuccessEvent += (int idx) => {
            if (idx == -1 && spellPath.Count > 0) {  // Tap on screen to finalize cast
                gestureSystem.disableGestureDrawing(); // Disable drawing
                // Create aim system that corresponds to this spell
                GameObject aimSystemObject = Instantiate(getNodeFromSequence(spellPath).getValue().AimSystemPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                AAimSystem aimSystem = aimSystemObject.GetComponent<AAimSystem>();
                aimSystem.setPlayerTransform(ownPlayerTransform);
                aimSystem.AimingFinishedEvent += (INetworkSerializable spellData) => {  // Once aiming finishes
                    SpawnSpellNormalServerRpc(spellPath.ToArray(), NetworkManager.Singleton.LocalClientId, spellData);  // Spawn spell
                    ClearSpellPath();  // Clear path
                    gestureSystem.enableGestureDrawing();  // Re-enable gesture drawing
                };
            } else {  // Drew an actual gesture, append it and reset time
                AddToSpellPath(idx);
            }
        };
        gestureSystem.GestureBackfire += (int idx) => {
            // Spawn backfired spell and clear path
            AddToSpellPath(idx, fireEvent:false);
            SpawnSpellBackfireServerRpc(spellPath.ToArray(), NetworkManager.Singleton.LocalClientId);
            ClearSpellPath();
        };
        gestureSystem.GestureFail += (int idx) => {
            // Gesture failed, clear the path
            ClearSpellPath();
        };
    }

    private void Update() {
        // Clear path if time ran out
        if (spellPath.Count > 0 && Time.time - timestamp > TIME_BETWEEN_GESTURES) {ClearSpellPath();}
    }

    /** Clears the spell path, fires appropriate event, and resets timestamp */
    private void ClearSpellPath() {
        spellPath.Clear();
        GestureSequenceClearEvent();
        timestamp = Time.time;
    }

    /** Adds a single index to spell path, fires appropriate event, and resets timestamp */
    private void AddToSpellPath(int idx, bool fireEvent = true) {
        spellPath.Add(idx);
        if (fireEvent) {GestureSequenceAdd(getNodeFromSequence(spellPath).getValue().Gesture);}
        timestamp = Time.time;
    }

    /** Sets spell path to given sequence, fires appropriate event, and resets timestamp */
    private void SetSpellPath(List<int> idxSeq) {
        spellPath = idxSeq;
        GestureSequenceSet(getCurrentChildren());
        timestamp = Time.time;
    }

    /** 
    * Return the node at the given seq, where seq is a sequence of indexes into children of the tree starting from the root
    */
    private SpellTreeDS getNodeFromSequence(List<int> seq) {
        SpellTreeDS cur = spellTreeRoot;
        foreach (int i in seq) {cur = spellTreeRoot.getChild(i);}
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
    * :param spellParams: A descendent serializable type for the parameters needed by this spell
    */
    [ServerRpc]
    private void SpawnSpellNormalServerRpc(int[] treeIndexSequence, ulong playerId, INetworkSerializable spellParams) {
        SpawnSpellServerSide(treeIndexSequence, playerId, spellParams);
    }

    /**
    * :param treeIndexSequence: index into spellTree to access the spellDS that was cast
    * :param playerId: Netcode id of the client
    */
    [ServerRpc]
    private void SpawnSpellBackfireServerRpc(int[] treeIndexSequence, ulong playerId) {
        SpawnSpellServerSide(treeIndexSequence, playerId, null);
    }

    /** Called by the two server RPCs, should NOT be called by anything else */
    private void SpawnSpellServerSide(int[] treeIndexSequence, ulong playerId, INetworkSerializable? spellParams) {
        if (IsServer) {
            // Find spell via tree index sequence
            SpellTreeDS cur = getNodeFromSequence(treeIndexSequence.ToList());

            // Instantiate and spawn on network
            GameObject spell = Instantiate(cur.getValue().SpellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spell.GetComponent<ISpell>().setPlayerId(playerId);
            if (spellParams != null) {spell.GetComponent<ISpell>().setParams(spellParams);
            } else { spell.GetComponent<ISpell>().setParams();}
            
            // Spawn to all clients
            spell.GetComponent<NetworkObject>().Spawn();
        }
    }
}
