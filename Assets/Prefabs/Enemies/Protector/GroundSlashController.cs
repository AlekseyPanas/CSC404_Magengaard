using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GroundSlashController : NetworkBehaviour
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] float speed;
    [SerializeField] float duration;
    [SerializeField] float damage;
    List<GameObject> collided;
    Vector3 _dir;
    void Start()
    {
        collided = new List<GameObject>();
        if(_dir != null) StartCoroutine(Shoot(speed));
        Destroy(gameObject, duration + 3f);
    }
    public void SetDirection(Vector3 dir){
        _dir = dir;
    }

    IEnumerator Shoot(float speed){
        _rb.velocity = _dir * speed;
        float time = duration;
        while(time > 0){
            _rb.velocity = Mathf.Lerp(0, speed, time / duration) * _dir;
            time -= Time.deltaTime; 
            yield return new WaitForEndOfFrame();
        }
        _rb.velocity = Vector3.zero;
    }

    void OnTriggerEnter(Collider col){
        if(!collided.Contains(col.gameObject)){
            if(col.CompareTag("Player")){
                IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)damage, SourcePosition = transform.position });
            }
            if(col.CompareTag("Ground") || col.CompareTag("Player")){
                _rb.velocity = Vector3.zero;
            }
            collided.Add(col.gameObject);
        }
    }
}
