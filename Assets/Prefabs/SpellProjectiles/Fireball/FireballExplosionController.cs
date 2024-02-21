using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class FireballExplosionController : NetworkBehaviour
{
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    public ulong playerID;
    private List<GameObject> alreadyCollided;
    int frameSinceSpawn = 0;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
        alreadyCollided = new List<GameObject>();
    }
    void OnTriggerEnter(Collider col){
        if(!IsServer) return;
        if ((col.gameObject.CompareTag("Player") && 
            col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID) || 
            col.gameObject.CompareTag("Enemy")){
            if (!alreadyCollided.Contains(col.gameObject)){
                IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect{Amount = (int)damage});
                alreadyCollided.Add(col.gameObject);
            }
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
