using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float interval;
    private float timer;
    public GameObject enemyToSpawn;
    public PlayerDetector pd;
    
    void Start()
    {
        timer = Time.time;
    }

    void Update()
    {
        if(Time.time > timer){
            timer = Time.time + interval;
            GameObject s = Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            s.GetComponent<NetworkObject>().Spawn();
            AggroPlayerDetector a = s.GetComponent<AggroPlayerDetector>();
            a.pd = pd;
            GameObject player = pd.GetPlayer();
            if(player != null){
                a.TriggerAggroEvent(player);
            }
        }
    }
}
