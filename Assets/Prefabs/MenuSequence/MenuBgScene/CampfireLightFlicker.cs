using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireLightFlicker : MonoBehaviour
{
    [SerializeField] private float flickerStrength; 
    [SerializeField] private float flickerSpeed; 
    private float baseIntensity;

    // Start is called before the first frame update
    void Start() {
        baseIntensity = GetComponent<Light>().intensity;
    }

    // Update is called once per frame
    void Update() {
        Light light = GetComponent<Light>();
        float diff = Mathf.PerlinNoise1D(Time.time * flickerSpeed) * (2 * flickerStrength) - flickerStrength;
        light.intensity = baseIntensity + diff;
    }
}
