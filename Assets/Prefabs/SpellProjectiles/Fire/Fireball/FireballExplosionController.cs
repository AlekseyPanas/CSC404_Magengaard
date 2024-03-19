using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class FireballExplosionController : NetworkBehaviour
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private float tempDelta;
    public ulong playerID;
    private List<GameObject> alreadyCollided;
    int frameSinceSpawn = 0;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
        alreadyCollided = new List<GameObject>();
    }
    void OnTriggerEnter(Collider col){
        if(!IsServer || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        if (!alreadyCollided.Contains(col.gameObject)){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect{Amount = (int)damage, SourcePosition = transform.position});
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect{TempDelta = (int)tempDelta, mesh = gameObject});
            alreadyCollided.Add(col.gameObject);
        }
    }
    void DestroySpell(){
        if(!IsOwner) return;
        Destroy(gameObject);
    }
    void Update(){
        if (frameSinceSpawn == 10) {
            GetComponent<Collider>().enabled = false;
        } else {
            frameSinceSpawn++;
        }
    }
}
