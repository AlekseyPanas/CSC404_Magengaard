using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireLightFlicker : MonoBehaviour
{
    [SerializeField] private float flickerStrength; 
    [SerializeField] private float flickerSpeed; 
    private float baseIntensity;
    private Light light;

    // Start is called before the first frame update
    void Start() {
        baseIntensity = GetComponent<Light>().intensity;
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update() {
        float diff = Mathf.PerlinNoise1D(Time.time * flickerSpeed) * (2 * flickerStrength) - flickerStrength;
        light.intensity = baseIntensity + diff;
    }
}
