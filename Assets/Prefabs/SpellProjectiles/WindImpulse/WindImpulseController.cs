using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WindImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    private Vector3 dir;
    public ulong playerID;
    [SerializeField] private float windEffectSpeed;

    [SerializeField] private float timer;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
        GetComponent<SphereCollider>().enabled = false;
        timer += Time.time;
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner) return;
        Vector3 dir = (col.gameObject.transform.position - transform.position).normalized;
        IEffectListener<WindEffect>.sendEffect(col.gameObject, new WindEffect().setWindVelocity(dir * windEffectSpeed));
        Debug.Log("sending wind effect");
    }

    void DestroySpell(){
        if(!IsOwner) return;
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        //Debug.Log("Are we on the server? " + IsServer);  // NO
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;
        
        Direction3DSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
    }

    public void postInit() {
        //Debug.Log("Are we on the server NOW? " + IsServer);  // YES
        Vector3.Normalize(dir);
        dir = new Vector3(dir.x, 0, dir.z);
        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
    }

    public void Update(){
        if (Time.time > timer){
            GetComponent<SphereCollider>().enabled = true;
        }
    }
}
