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
    void Start()
    {
        
    }

    void Update()
    {
        // Testing Spells
        if (IsOwner) {
            Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
            {
                difference = hit.point - player.transform.position;
                direction = new Vector3(difference.x, 0, difference.z).normalized;
            }
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.FIREBALL, direction, new ServerRpcParams());
            }
            if(Input.GetKeyDown(KeyCode.Alpha2)){
                SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.ELECTROSPHERE, hit.point, new ServerRpcParams());
            }
            if(Input.GetKeyDown(KeyCode.Alpha3)){
                SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.EARTHENWALL, hit.point, new ServerRpcParams());
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.SANDSTORM, new Vector3(1, 0, 1), new ServerRpcParams());
            }
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    // void CastFireballServerRpc() {
    //     Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
    //     if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
    //     {
    //         difference = hit.point - player.transform.position;
    //         direction = new Vector3(difference.x, 0, difference.z).normalized;
    //         Debug.Log("casting fireball");
    //         SpellFactory.instance.SpellLinearProjectileServerRpc(SpellFactory.SpellId.FIREBALL, direction, new ServerRpcParams());
    //     }
    //     canShoot = false;
    //     Invoke("ResetGesture", 1);
    // }
    // [ServerRpc(RequireOwnership = false)]
    // void CastElectroSphereServerRpc() {
    //     Ray ray = playerCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
    //     if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, layermask))
    //     {
    //         SpellFactory.instance.SpellRemoteServerRpc(SpellFactory.SpellId.ELECTROSPHERE, hit.point, new ServerRpcParams());
    //     }
    //     canShoot = false;
    //     Invoke("ResetGesture", 1);
    // }

    void ResetGesture(){
        gs.isReadyToFire = false;
    }
}
