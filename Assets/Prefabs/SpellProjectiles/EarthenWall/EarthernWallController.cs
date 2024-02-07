using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EarthernWallController : NetworkBehaviour, ISpell
{
    [SerializeField] float lifeTime;
    ulong playerID;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
    }

    void DestroySpell(){
        if(!IsOwner) return;
        // do the animations here, can call an animation and actually delete the object via animation event
        Destroy(gameObject);
    }
    public void preInitSpell()
    {        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject player = null;
        foreach(GameObject p in players){
            if (p.GetComponent<NetworkBehaviour>().OwnerClientId == playerID){
                player = p;
            }
        }
        Vector3 diff = (transform.position - player.transform.position).normalized;
        transform.right = new Vector3(diff.x, 0, diff.z);
    }

    public void setPlayerId(ulong playerId)
    {
        playerID = playerId;
    }

    public void setParams(Direction2DSpellParams spellParams)
    {
        throw new System.NotImplementedException();
    }

    public void setParams()
    {
        throw new System.NotImplementedException();
    }
}
