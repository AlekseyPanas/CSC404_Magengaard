using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellFactory : NetworkBehaviour
{

    public static SpellFactory instance;

    public enum SpellId {
        SANDSTORM = 1
    }

    // Enum mapping to each prefab script component
    private Dictionary<SpellId, GameObject> spellIdToPrefab;

    // Spell prefab fields
    [SerializeField] private GameObject sandstormPrefab;

    // Start is called before the first frame update
    void Start() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }
        instance = this;

        spellIdToPrefab = new Dictionary<SpellId, GameObject>() {{SpellId.SANDSTORM, sandstormPrefab}};
    }

    // Update is called once per frame
    void Update() {}

    [ServerRpc]
    public void SpellLinearProjectileServerRpc(SpellId spellId, Vector3 direction, ServerRpcParams rpcparams) {
        PrefabFactory.SpawnLinearProjectileSpell(spellIdToPrefab[spellId], rpcparams.Receive.SenderClientId, direction);
    }

    [ServerRpc]
    public void SpellRemoteServerRpc(SpellId spellId, Vector3 origin, ServerRpcParams rpcparams) {
        PrefabFactory.SpawnRemoteOriginSpell(spellIdToPrefab[spellId], rpcparams.Receive.SenderClientId, origin);
    }
}
