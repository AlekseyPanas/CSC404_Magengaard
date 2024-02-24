using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private float temperature;
    private Vector3 dir;
    public ulong playerID;
    float timer = 0.1f;
    List<GameObject> objectsAlreadyCollided;
    void Awake(){
        Invoke("DestroySpell", lifeTime);
        timer += Time.time;
        objectsAlreadyCollided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if (!IsOwner) return;
        if ((col.gameObject.CompareTag("Player") && 
                col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID) || 
                col.gameObject.CompareTag("Enemy")){
            if (!objectsAlreadyCollided.Contains(col.gameObject)){
                Vector3 dir = col.gameObject.transform.position - transform.position;
                dir = new Vector3(dir.x, 0, dir.z).normalized;
                IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature});
                IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int) damage});
                objectsAlreadyCollided.Add(col.gameObject);
            }
        }
    }

    void DestroySpell(){
        if(!IsOwner) return;
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;

        Direction3DSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
    }

    public void postInit() {
        Vector3.Normalize(dir);
        dir = new Vector3(dir.x, 0, dir.z);
        transform.position = player.transform.position + dir * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER; //second number in the vector should be around the height of the player's waist
    }

    public void Update(){
        if (Time.time > timer){
            GetComponent<Collider>().enabled = false;
        }
    }
}
