using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bin3Particles : MonoBehaviour
{
    public List<ParticleSystem> particles;
    public float lifetime;
    public float timer = 0;
    public Vector3 targetPos;

    void Start(){
        StartCoroutine(OnSpawn());
        transform.localScale *= 0.5f;
    }

    IEnumerator OnSpawn(){
        Vector3 dir = new Vector3(Random.Range(-1,1), Random.Range(-1,1), 0) * 0.3f;
        Vector3 startPos = transform.position;
        targetPos = dir * Random.Range(0.5f,1.5f) + startPos;
        while(timer < lifetime){
            transform.position = Vector3.Lerp(startPos, targetPos, timer/lifetime);
            foreach (ParticleSystem ps in particles) {
                var main = ps.main;
                Color c = main.startColor.color;
                main.startColor = new Color(c.r, c.g, c.b, 1 - timer / lifetime);
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach(ParticleSystem ps in particles){
            var e = ps.emission;
            e.enabled = false;
        }
        Destroy(gameObject, 5f);
    }
}
