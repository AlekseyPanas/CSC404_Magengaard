using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public float interval;
    private float timer;
    public AEnemy enemyToSpawn;
    public PlayerDetector pd;
    
    void Start()
    {
        timer = Time.time;
    }

    void Update()
    {
        if(!IsServer) return;
        if(Time.time > timer){
            timer = Time.time + interval;
            GameObject s = Instantiate(enemyToSpawn.gameObject, transform.position, Quaternion.identity);
            s.GetComponent<NetworkObject>().Spawn();
            AggroPlayerDetector a = s.GetComponent<AggroPlayerDetector>();
            a.pd = pd;
            GameObject player = pd.GetPlayer();
            if(player != null){
                s.GetComponent<AEnemy>().SubscribeToAggroEvent(); // have to do this before triggering the aggro event to insure the enemy is subscribed
                a.TriggerAggroEvent(player);
            }
        }
    }
}
