using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class IceCubeIceEffect : NetworkBehaviour
{
    [SerializeField] float _temperature;
    public float playerID;

    void OnTriggerStay(Collider col) {
        if (!IsOwner || (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId == playerID)) return;
        IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = _temperature});
    } 
}
