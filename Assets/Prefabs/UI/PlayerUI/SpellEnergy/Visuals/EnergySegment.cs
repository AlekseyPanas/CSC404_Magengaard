using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySegment : MonoBehaviour
{
    [SerializeField] GameObject pivot;
    [SerializeField] SpriteRenderer energyBar;
    [SerializeField] List<ParticleSystem> particles;


    public void OnDeplete(){
        StartCoroutine(Shrink());
    }
    IEnumerator Shrink() {
        float timer = 0;
        float duration = 0.2f;
        while(timer < duration) {
            pivot.transform.localScale = new Vector3(Mathf.Lerp(1, 0, timer/duration), 1, 1);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }        
        Destroy(gameObject);
    }

    public void SetColor(Color c) {
        List<Material> m = new List<Material>();
        energyBar.GetMaterials(m);
        m[0].SetColor("_BaseMapColor", c);
        m[0].SetColor("_EmissionColor", c);
        foreach (ParticleSystem p in particles) {
            var main = p.main;
            main.startColor = ConvertColor(c, main.startColor.color);
            foreach(ParticleSystem cp in p.transform.GetComponentsInChildren<ParticleSystem>()){
                var cp_main = cp.main;
                cp_main.startColor = ConvertColor(c, cp_main.startColor.color);
            }
        }
    }

    public Color ConvertColor(Color newColor, Color oldColor){
        Color.RGBToHSV(newColor, out var hue, out var s, out var v);
        Color.RGBToHSV(oldColor, out var h, out var sat, out var val);
        return Color.HSVToRGB(hue, sat, val);
    }
}
