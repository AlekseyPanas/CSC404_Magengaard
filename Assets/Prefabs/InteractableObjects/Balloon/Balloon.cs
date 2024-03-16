using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Balloon : NetworkBehaviour, IEffectListener<TemperatureEffect>
{
    private Rigidbody _body;

    private bool _lit = false;
    public GameObject fire;
    
    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        
        if (_lit)
        {
            _body.AddForce(Vector3.up * 1000f * Time.deltaTime);
        }
    }

    void Light()
    {
        _lit = true;
        fire.SetActive(true);
        
        _body.AddForce(Vector3.up * 2000f);
    }

    [ClientRpc]
    void LightClientRpc()
    {
        Light();
    }
    
    [ServerRpc]
    void LightServerRpc()
    {
        Light(); // Call on Server.
        
        LightClientRpc();
    }

    public void OnEffect(TemperatureEffect effect)
    {
        if (!IsServer) // does OnEffect get administered on clients?
        {
            return;
        }

        if (effect.TempDelta > 3)
        {
            LightServerRpc();
        }
    }
}
