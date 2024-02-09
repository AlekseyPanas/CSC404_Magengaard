using UnityEngine;
using Unity.Netcode;

public class EarthernWallController : NetworkBehaviour, ISpell
{
    [SerializeField] float lifeTime;
    ulong playerID;
    private Vector3 _direction;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
    }

    void DestroySpell(){
        if(!IsOwner) return;
        // do the animations here, can call an animation and actually delete the object via animation event
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() {}

    public void preInit(SpellParamsContainer spellParams) {
        // Construct the correct parameters
        Direction3DSpellParams prms = new Direction3DSpellParams();
        prms.buildFromContainer(spellParams);
        _direction = new Vector3(prms.Direction3D.x, 0, prms.Direction3D.z).normalized;

        GameObject player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;
        transform.right = _direction;
    }

    public void postInit() { }
}
