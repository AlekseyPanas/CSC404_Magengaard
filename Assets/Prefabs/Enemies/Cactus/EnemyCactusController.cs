using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyCactusController : AEnemy, IEffectListener<DamageEffect>, IEffectListener<WindEffect> {
    float attackTimer = 0;
    float distanceToPlayer;
    float patrolTimer = 0;
    Vector3 patrolCenter;
    
    [SerializeField] private float patrolRadius; //radius of which the enemy randomly moves while idle
    [SerializeField] private float patrolMoveSpeed;
    [SerializeField] private float patrolPositionChangeInterval;
    [SerializeField] private float chaseRadius;
    [SerializeField] private float chaseMoveSpeed;
    [SerializeField] private float backOffRadius;
    [SerializeField] private float backOffMoveSpeed;
    [SerializeField] private float attackRange; //range at which the enemy must be from the player to attack
    [SerializeField] private float attackInterval; //amount of time between each attack, will be randomized slighly.
    [SerializeField] private Transform projectileSpawnPos;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private float kbMultiplier;
    [SerializeField] private float kbDuration;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private bool isDefendPosition;
    [SerializeField] private GameObject defendPoint;
    [SerializeField] private float defendRange;
    public GameObject attackProjectile;
    Vector3 chaseOffset;
    Vector3 offsetVector;
    Vector3 diff;
    bool resetChaseOffset = true;

    void Start() {
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
        currHP = maxHP;
        playerDetector.OnPlayerEnter += OnPlayerEnter;
    }

    /** 
    * Fires event and cleans up, destroys this game object
    */
    void Death(){
        invokeDeathEvent();
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        playerDetector.OnPlayerEnter -= OnPlayerEnter;
        Destroy(gameObject);
    }
    
    public void OnEffect(DamageEffect effect) {
        currHP -= effect.Amount;
        if (currHP <= 0) {
            Death();
        }
        UpdateHPBar();
    }

    /**
    * Knockback effect from wind
    */
    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
    }

    void KnockBack(Vector3 dir){
        agent.enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * kbMultiplier, ForceMode.Impulse);
        Invoke("ResetKnockBack", kbDuration);
    }

    void OnPlayerEnter(GameObject g) { TryAggro(g); }

    protected override void OnDeAggro() { agent.speed = patrolMoveSpeed; }

    protected override void OnNewAggro() { SetChaseInfo(); }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;
        if(agent.enabled){
            if (GetCurrentAggro() != null) {
                ChasePlayer();
            } else {
                Patrol();
            }
        }
        hpbarCanvas.transform.LookAt(Camera.main.transform);
        // animation stuff
        if (agent.velocity.magnitude < 0.05f) {
            anim.SetBool("isMoving", false);
        } else {
            anim.SetBool("isMoving", true);
        }
    }

    void UpdateHPBar(){
        hpbarfill.GetComponent<Image>().fillAmount = currHP/maxHP;
    }

    void ResetKnockBack(){
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.enabled = true;
    }

    void SetChaseInfo() { agent.speed = chaseMoveSpeed; }

    void Patrol(){
        if(Time.time > patrolTimer){
            float r = UnityEngine.Random.Range(0f,1f);
            if (r < 0.7f){
                float randX = UnityEngine.Random.Range(-patrolRadius, patrolRadius);
                float randZ = UnityEngine.Random.Range(-patrolRadius, patrolRadius);
                agent.SetDestination(patrolCenter + new Vector3(randX, 0, randZ));
            }
            patrolTimer = Time.time + patrolPositionChangeInterval * UnityEngine.Random.Range(-0.5f, 1.5f);
        }   
    }
    void ChasePlayer(){
        diff = GetCurrentAggro().position - transform.position;
        distanceToPlayer = diff.magnitude;
        diff = new Vector3(diff.x, 0, diff.z);
        transform.forward = diff.normalized;
        if(isDefendPosition &&
            (transform.position - defendPoint.transform.position).magnitude > defendRange){
            RetreatToDefensePosition();
        } else {
            if (distanceToPlayer > chaseRadius){ // need to move closer to player
                if(resetChaseOffset){
                    resetChaseOffset = false;
                    offsetVector = Vector3.Cross(diff, Vector3.up).normalized * UnityEngine.Random.Range(-2f,2f);
                }
                chaseOffset = (diff + offsetVector).normalized * chaseRadius;
                MoveToPlayer();
            } else if (distanceToPlayer < backOffRadius) { // too close, need to back off
                BackOff(diff.normalized);
            }
        }
        if (distanceToPlayer <= attackRange) { // can attack, need to check timer
            if (Time.time >= attackTimer) { //can attack
                SetAnimShoot();
            }
        }
    }

    void RetreatToDefensePosition(){
        agent.SetDestination(defendPoint.transform.position);
    }

    void SetAnimShoot(){
        anim.SetBool("isShooting", true);
    }

    public void ResetSpeed(){
        if(GetCurrentAggro() == null){
            agent.speed = patrolMoveSpeed;
        }else{
            agent.speed = chaseMoveSpeed;
        }
    }

    public void AttackPlayer(){
        if(GetCurrentAggro() == null) return;
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
        GameObject proj = Instantiate(attackProjectile, projectileSpawnPos.position, Quaternion.identity); //projectile behaviour will be handled on the projectile object
        Vector3 shootDir = (GetCurrentAggro().position - transform.position).normalized;
        proj.GetComponent<EnemyCactusProjectileController>().SetTargetDirection(shootDir);
        proj.GetComponent<NetworkObject>().Spawn();
    }

    public void SlowSpeed(){
        agent.speed = 0.1f;
    }

    void MoveToPlayer(){
        if (Physics.Raycast(transform.position, diff, out var hit, Mathf.Infinity) && hit.transform.CompareTag("Ground")) {
            resetChaseOffset = true;
            agent.stoppingDistance = chaseRadius;
            agent.SetDestination(GetCurrentAggro().position);
        } else {
            // if nothing is blocking
            agent.stoppingDistance = 0;
            agent.SetDestination(GetCurrentAggro().position - chaseOffset); //i have no idea why chaseOffset has to be subtracted here. if it is added, the offset goes past the player
        }
    }

    void BackOff(Vector3 dir){
        resetChaseOffset = true;
        agent.speed = backOffMoveSpeed;
        agent.stoppingDistance = chaseRadius;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
