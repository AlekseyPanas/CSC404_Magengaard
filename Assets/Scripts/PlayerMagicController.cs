using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMagicController : NetworkBehaviour
{
    public GameObject player;
    public GameObject playerCam;
    [SerializeField]
    public LayerMask layermask;
    public GestureSystem gs;
    public bool canShoot = false;
    Vector3 difference;
    Vector3 direction;
    bool canFireSpell = false;
    DesktopControls controls;
    SpellFactory.SpellId spellId;
    void Start()
    {
        controls = new DesktopControls();
        controls.Enable();
        controls.Game.Enable();
        controls.Game.Fire.performed += _ => CastSpellWithID();
    }

    void Awake(){
        GameObject.FindWithTag("GestureCanvas").GetComponent<GestureSystem>().SetOverlayCameraStack(playerCam.GetComponent<Camera>());
        GestureSystem.CastSpell += (SpellFactory.SpellId spellId) =>
        {
            canFireSpell = true;
            this.spellId = spellId;
        };
    }

    void CastSpellWithID()
    {
        if(!canFireSpell) return;
        Debug.Log("fire spell with id" + spellId);
        if (IsOwner) {
            Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
            {
                difference = hit.point - player.transform.position;
                direction = new Vector3(difference.x, 0, difference.z).normalized;
            }
            if(spellId == SpellFactory.SpellId.FIREBALL){
                SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.FIREBALL, difference.normalized, new ServerRpcParams());
            }
            if(spellId == SpellFactory.SpellId.ELECTROSPHERE){
                SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.ELECTROSPHERE, hit.point, new ServerRpcParams());
            }
            if(spellId == SpellFactory.SpellId.EARTHENWALL){
                SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.EARTHENWALL, hit.point, new ServerRpcParams());
            }
            if (spellId == SpellFactory.SpellId.SANDSTORM){
                SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.SANDSTORM, direction, new ServerRpcParams());
            }
            canFireSpell = false;
        }
    }
}
