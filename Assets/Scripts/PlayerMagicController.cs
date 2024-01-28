using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMagicController : NetworkBehaviour
{
    public GameObject fireball;
    public GameObject electrosphere;
    public GameObject player;
    public GameObject playerCam;
    [SerializeField]
    private LayerMask layermask;
    private Vector3 difference;
    private Vector3 direction;
    public GestureSystem gs;
    public bool canShoot = false;
    void Start()
    {
        
    }

    void Update()
    {
        // Testing Sandstorm Spell
        if (IsOwner) {
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                //if(canShoot){
                    SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.FIREBALL, new Vector3(1,0,-1), new ServerRpcParams());
                //}
            }
            if(Input.GetKeyDown(KeyCode.Alpha2)){
                //if(canShoot){
                    SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.ELECTROSPHERE, transform.position, new ServerRpcParams());
                //}
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.SANDSTORM, new Vector3(1, 0, 1), new ServerRpcParams());
                // canShoot = false;
                // Invoke("ResetGesture", 1);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void CastFireballServerRpc() {
        Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
        {
            difference = hit.point - player.transform.position;
            direction = new Vector3(difference.x, 0, difference.z).normalized;
            Debug.Log("casting fireball");
            SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.FIREBALL, direction, new ServerRpcParams());
        }
        // canShoot = false;
        // Invoke("ResetGesture", 1);
    }
    [ServerRpc(RequireOwnership = false)]
    void CastElectroSphereServerRpc() {
        Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
        {
            SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.ELECTROSPHERE, hit.point, new ServerRpcParams());
        }
        // canShoot = false;
        // Invoke("ResetGesture", 1);
    }

    void ResetGesture(){
        gs.isReadyToFire = false;
    }
}
