using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IceImpulseController : NetworkBehaviour, ISpell
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private float temperature;
    private Vector3 dir;
    public ulong playerID;
    float timer = 0.1f;
    public List<GameObject> objectsAlreadyCollided;
    void Awake(){
        Invoke("DestroySpell", lifeTime);
        timer += Time.time;
        objectsAlreadyCollided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        Debug.Log(col.name);
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!objectsAlreadyCollided.Contains(col.gameObject)){
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature});
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int) damage, SourcePosition = transform.position});
            objectsAlreadyCollided.Add(col.gameObject);
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
