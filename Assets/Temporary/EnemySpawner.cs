using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public event Action OnEnemiesCleared;
    public float interval;
    private float timer;
    public AEnemy enemyToSpawn;
    public PlayerDetector pd;
    public List<GameObject> spawnedEnemies = new();
    public bool spawnOverTime;
    public bool enableEnemiesOnSpawn;

    
    void Start()
    {
        timer = Time.time;
    }

    public void SpawnNumEnemies(int num){
        for(int i = 0; i < num; i++){
            Vector3 pos = new(UnityEngine.Random.Range(0f,1f), 0, UnityEngine.Random.Range(0f,1f));
            spawnedEnemies.Add(SpawnEnemy(false, pos));
        }
    }

    public void RemoveEnemyFromList(GameObject g){
        spawnedEnemies.Remove(g);
        if(spawnedEnemies.Count == 0){
            OnEnemiesCleared?.Invoke();
        }
    }

    void Update()
    {
        if(!IsServer || !spawnOverTime) return;
        if(Time.time > timer){
            timer = Time.time + interval;
            SpawnEnemy(enableEnemiesOnSpawn, Vector3.zero);
        }
    }

    GameObject SpawnEnemy(bool isEnabled, Vector3 offset){
        GameObject s = Instantiate(enemyToSpawn.gameObject, transform.position + offset, Quaternion.identity);
        s.GetComponent<NetworkObject>().Spawn();
        AggroPlayerDetector a = s.GetComponent<AggroPlayerDetector>();
        a.pd = pd;
        GameObject player = pd.GetPlayer();
        AEnemy enemy = s.GetComponent<AEnemy>();
        if(player != null){
            enemy.SubscribeToAggroEvent(); // have to do this before triggering the aggro event to insure the enemy is subscribed
            if (isEnabled) {
                a.TriggerAggroEvent(player);
            } else {
                a.SetAgroOnSpawn(false);
            }
        }
        enemy.SetAIEnabledOnSpawn(isEnabled);
        enemy.OnDeath += RemoveEnemyFromList;
        return s;
    }
    
    public void EnableEnemiesAI(GameObject player){
        foreach(GameObject g in spawnedEnemies){
            g.GetComponent<AEnemy>().EnableAgent();
            g.GetComponent<AggroPlayerDetector>().TriggerAggroEvent(player);
        }
    }
}
