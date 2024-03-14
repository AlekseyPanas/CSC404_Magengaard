using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireSpriteProjectileController : NetworkBehaviour
{
    [SerializeField] float timer;
    [SerializeField] float damageInterval;
    [SerializeField] float damage;
    [SerializeField] List<GameObject> currentlyColliding;
    [SerializeField] ParticleSystem ps;
    [SerializeField] Collider col;
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
        if (currentlyColliding.Count > 0) {
            IEffectListener<DamageEffect>.SendEffect(currentlyColliding, new DamageEffect{Amount = (int)damage, SourcePosition = transform.position});
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.CompareTag("Player")){
            if (!currentlyColliding.Contains(col.gameObject)){
                currentlyColliding.Add(col.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider col){
        if (col.CompareTag("Player")){
            if (currentlyColliding.Contains(col.gameObject)){
                currentlyColliding.Remove(col.gameObject);
            }
        }
    }

    public void StartAttack(){
        ps.Play();
        col.enabled = true;
    }

    public void EndAttack(){
        ps.Stop();
        col.enabled = false;
        currentlyColliding.Clear();
    }
}
