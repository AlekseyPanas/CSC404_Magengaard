using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellFactory : NetworkBehaviour
{

    public static SpellFactory instance;

    public enum SpellId {
        SANDSTORM = 1,
        FIREBALL = 2,
        ELECTROSPHERE = 3
    }

    // Enum mapping to each prefab script component
    private Dictionary<SpellId, GameObject> spellIdToPrefab;

    // Spell prefab fields
    [SerializeField] private GameObject sandstormPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject electroSpherePrefab;

    // Start is called before the first frame update
    void Start() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }
        instance = this;

        spellIdToPrefab = new Dictionary<SpellId, GameObject>() {
            {SpellId.SANDSTORM, sandstormPrefab}, 
            {SpellId.FIREBALL, fireballPrefab}, 
            {SpellId.ELECTROSPHERE, electroSpherePrefab}
        };
    }

    // Update is called once per frame
    void Update() {}

    [ServerRpc(RequireOwnership = false)]
    public void SpellLinearProjectileServerRpc(SpellId spellId, Vector3 direction, ServerRpcParams rpcparams) {
        PrefabFactory.SpawnLinearProjectileSpell(spellIdToPrefab[spellId], rpcparams.Receive.SenderClientId, direction);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpellRemoteServerRpc(SpellId spellId, Vector3 origin, ServerRpcParams rpcparams) {
        PrefabFactory.SpawnRemoteOriginSpell(spellIdToPrefab[spellId], rpcparams.Receive.SenderClientId, origin);
    }
}
