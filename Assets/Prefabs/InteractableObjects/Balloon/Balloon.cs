using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, IEffectListener<TemperatureEffect>
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

    public void OnEffect(TemperatureEffect effect)
    {
        if (effect.TempDelta > 3)
        {
            Light();
        }
    }
}
