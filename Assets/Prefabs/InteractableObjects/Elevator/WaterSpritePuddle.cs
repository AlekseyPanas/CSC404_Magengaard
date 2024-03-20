using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class WaterSpritePuddle : NetworkBehaviour
{
    public Animator puddle;
    public GameObject sprite;

    public PlayerDetector detector;

    public UnityEvent onDeath;
    
    private static readonly int Dry = Animator.StringToHash("Dry");

    public void Spawn()
    {
        if (!IsServer)
        {
            return;
        }
        
        puddle.SetBool(Dry, true);
        
        // From EnemySpawner.cs
        var s = Instantiate(sprite, transform.position, Quaternion.identity);
        s.GetComponent<NetworkObject>().Spawn();
        var a = s.GetComponent<AggroPlayerDetector>();
        a.pd = detector;
        a.SetAgroOnSpawn(true);

        var player = detector.GetPlayer();
        
        var enemy = s.GetComponent<AEnemy>();
        enemy.SetAIEnabledOnSpawn(true);

        enemy.OnDeath += _ => onDeath.Invoke();
        
        if (player != null){
            enemy.SubscribeToAggroEvent();
            a.TriggerAggroEvent(player);
        }
    }
}
