using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public float interval;
    private float timer;
    public GameObject enemy;
    public PlayerDetector pd;
    // Start is called before the first frame update
    void Start()
    {
        timer = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timer){
            timer = Time.time + interval;
            GameObject s = Instantiate(enemy, transform.position, Quaternion.identity);
            s.GetComponent<AEnemy>().OnSpawnViaSpawner(pd);
            s.GetComponent<NetworkObject>().Spawn();
        }
    }
}
