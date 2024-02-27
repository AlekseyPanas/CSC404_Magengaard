using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireSpriteProjectileController : NetworkBehaviour
{
    public float lifetime;
    [SerializeField] float timer;
    [SerializeField] float damageInterval;
    [SerializeField] float damage;
    [SerializeField] List<GameObject> currentlyColliding;
    public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
        timer = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timer){
            timer = Time.time + damageInterval;
            DoDamage();
        }
        transform.position = parent.transform.position;
        transform.forward = parent.transform.forward;
    }

    void DoDamage(){
        if (currentlyColliding.Count > 0) {
            Debug.Log("dealing damage");
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
}
