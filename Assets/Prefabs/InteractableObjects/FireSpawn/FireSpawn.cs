using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FireSpawn : NetworkBehaviour
{
    public GameObject sprite;
    public PlayerDetector detector;

    private Animator _animator;
    public GameObject sphere;

    public UnityEvent onDeath;
    
    private static readonly int Active = Animator.StringToHash("Active");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void SpawnSprite() {
        // From EnemySpawner.cs
        var s = Instantiate(sprite, transform.position, Quaternion.identity);
        s.GetComponent<NetworkObject>().Spawn();
        var a = s.GetComponent<AggroPlayerDetector>();
        a.SetAgroOnSpawn(true);
        a.pd = detector;

        var player = detector.GetPlayer();
        
        var enemy = s.GetComponent<AEnemy>();
        enemy.SetAIEnabledOnSpawn(true);

        enemy.OnDeath += _ => onDeath.Invoke();
        
        if (player != null){
            enemy.SubscribeToAggroEvent();
            a.TriggerAggroEvent(player);
        }}
    
    public void Spawn()
    {
        sphere.SetActive(true);
        _animator.SetBool(Active, true);
        
        Invoke(nameof(SpawnSprite), 1.0f);
    }
}