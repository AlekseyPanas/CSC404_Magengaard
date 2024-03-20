using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireSpriteProjectileController : MonoBehaviour
{
    [SerializeField] float timer;
    [SerializeField] float damageInterval;
    [SerializeField] float damage;
    [SerializeField] float temperature;
    [SerializeField] List<GameObject> currentlyColliding;
    [SerializeField] ParticleSystem ps;
    [SerializeField] Collider col;
    [SerializeField] GameObject _fireMesh;
    public bool canAttack = true;
    void Start()
    {
        timer = Time.time;
        EndAttack();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timer){
            timer = Time.time + damageInterval;
            DoDamage();
        }
    }

    void DoDamage(){
        foreach(GameObject g in currentlyColliding){
            if (g!= null && !g.CompareTag("Enemy")) {
                IEffectListener<DamageEffect>.SendEffect(g, new DamageEffect{Amount = (int)damage, SourcePosition = transform.position});
                IEffectListener<TemperatureEffect>.SendEffect(g, new TemperatureEffect{TempDelta = temperature, mesh = _fireMesh});
            }
        }
    }

    void OnTriggerEnter(Collider col){
        if (!currentlyColliding.Contains(col.gameObject)){
            currentlyColliding.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col){
        if (currentlyColliding.Contains(col.gameObject)){
            currentlyColliding.Remove(col.gameObject);
        }
    }

    public void StartAttack(){
        if(canAttack){
            ps.Play();
            col.enabled = true; 
        }
    }

    public void EndAttack(){
        ps.Stop();
        col.enabled = false;
        currentlyColliding.Clear();
    }
}
