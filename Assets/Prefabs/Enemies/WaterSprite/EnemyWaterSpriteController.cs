using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyWaterSpriteController : AEnemyAffectedByElement, IEffectListener<TemperatureEffect>
{
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
    [SerializeField] private float burstInterval;
    [SerializeField] private int numShotsPerBurst;
    [SerializeField] private Transform projectileSpawnPos;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathParticles;
    public GameObject attackProjectile;
    Vector3 chaseOffset;
    Vector3 offsetVector;
    Vector3 diff;
    bool resetChaseOffset = true;

    new void Start() {
        base.Start();
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
        currHP = maxHP;
        agent.enabled = false;
        elementalResistances = new ElementalResistance(){fire = 0f, ice = 0.5f, wind = 0.5f, lightning = 0f};
    }
    public void OnSpawn(){
        if(AIEnabledOnSpawn) agent.enabled = true;
    }

    void Death(){
        invokeDeathEvent();
        GameObject g = Instantiate(deathParticles, transform.position + new Vector3(0,1,0), Quaternion.identity);
        g.GetComponent<NetworkObject>().Spawn();
        Destroy(gameObject);
    }
    
    public void OnEffect(TemperatureEffect effect)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        if(effect.TempDelta > 0) { // a fire attack
            currHP -= Mathf.Abs(effect.TempDelta) * (1 - elementalResistances.fire);
        } else { // an ice attack
            currHP -= Mathf.Abs(effect.TempDelta) * (1 - elementalResistances.ice);
        }
        if (currHP <= 0) {
            Death();
        }
        UpdateHPBar();
    }

    void OnPlayerEnter(GameObject player) { TryAggro(player); }

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
        if (agent.velocity.magnitude < 0.01f) {
            anim.SetBool("IsMoving", false);
        } else {
            anim.SetBool("IsMoving", true);
        }
    }

    void UpdateHPBar(){
        hpbarfill.GetComponent<Image>().fillAmount = currHP/maxHP;
    }

    void SetChaseInfo(){
        agent.speed = chaseMoveSpeed;
    }

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
        if (distanceToPlayer <= attackRange) { // can attack, need to check timer
            if (Time.time >= attackTimer) { //can attack
                SetAnimShoot();
            }
        }
    }

    void SetAnimShoot(){
        anim.SetTrigger("Attack");
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
    }

    public void ResetSpeed(){
        if(GetCurrentAggro() == null){
            agent.speed = patrolMoveSpeed;
        }else{
            agent.speed = chaseMoveSpeed;
        }
    }

    IEnumerator AttackPlayerBurst(){
        for(int i = 0; i < numShotsPerBurst; i++){
            GameObject proj = Instantiate(attackProjectile, projectileSpawnPos.position, Quaternion.identity); //projectile behaviour will be handled on the projectile object
            proj.GetComponent<NetworkObject>().Spawn();
            Transform agroTarget = GetCurrentAggro();
            if(agroTarget != null) {
                proj.GetComponent<WaterSpriteProjectileController>().player = agroTarget.gameObject;
            }
            yield return new WaitForSeconds(burstInterval);
        }
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
    }

    public void AttackPlayer(){
        //StartCoroutine(AttackPlayerBurst());
        GameObject proj = Instantiate(attackProjectile, projectileSpawnPos.position, Quaternion.identity); //projectile behaviour will be handled on the projectile object
        proj.GetComponent<NetworkObject>().Spawn();
        Transform agroTarget = GetCurrentAggro();
        if(agroTarget != null) { 
            proj.GetComponent<WaterSpriteProjectileController>().player = agroTarget.gameObject;
        }
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
