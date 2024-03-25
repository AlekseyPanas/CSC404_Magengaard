using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Balloon : NetworkBehaviour, IEffectListener<TemperatureEffect>
{
    private Rigidbody _body;

    private bool _lit = false;
    public GameObject fire;

    public UnityEvent reachedTop;
    public UnityEvent leftTop;

    private bool _atTop;

    public float lightForce = 20f;
    public float tickForce = 10f;

    public float maxHeight = float.PositiveInfinity;
    
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
            if (transform.position.y < maxHeight)
            {
                if (_atTop)
                {
                    _atTop = false;
                    
                    leftTop.Invoke();
                }
                
                _body.AddForce(Vector3.up * (tickForce * Time.deltaTime));
            }
            else
            {
                if (!_atTop)
                {
                    _atTop = true;
                    
                    reachedTop.Invoke();
                }
                _body.velocity = Vector3.zero;
            }
        }
    }

    void Light()
    {
        if (_lit)
        {
            return;
        }
        
        _lit = true;
        fire.SetActive(true);
        
        _body.AddForce(Vector3.up * lightForce);
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
