using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public float interval;
    private float timer;
    public AEnemy enemyToSpawn;
    public PlayerDetector pd;
    List<GameObject> spawnedEnemies = new();
    public bool spawnOverTime;

    
    void Start()
    {
        timer = Time.time;
    }

    public void SpawnNumEnemies(int num){
        for(int i = 0; i < num; i++){
            Vector3 pos = new(Random.Range(0f,1f),0,Random.Range(0f,1f));
            spawnedEnemies.Add(SpawnEnemy(false, pos));
        }
    }

    void Update()
    {
        if(!IsServer || !spawnOverTime) return;
        if(Time.time > timer){
            timer = Time.time + interval;
            SpawnEnemy(false, Vector3.zero);
        }
    }

    GameObject SpawnEnemy(bool isEnabled, Vector3 offset){
        GameObject s = Instantiate(enemyToSpawn.gameObject, transform.position + offset, Quaternion.identity);
        s.GetComponent<NetworkObject>().Spawn();
        AggroPlayerDetector a = s.GetComponent<AggroPlayerDetector>();
        a.pd = pd;
        GameObject player = pd.GetPlayer();
        if(player != null){
            s.GetComponent<AEnemy>().SubscribeToAggroEvent(); // have to do this before triggering the aggro event to insure the enemy is subscribed
            a.TriggerAggroEvent(player);
        }
        s.GetComponent<AEnemy>().SetAIEnabledOnSpawn(isEnabled);
        return s;
    }
    
    public void EnableEnemiesAI(){
        foreach(GameObject g in spawnedEnemies){
            g.GetComponent<AEnemy>().EnableAgent();
        }
    }
}
