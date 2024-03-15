using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyFireSpriteController : AEnemy, IEffectListener<WindEffect>, IEffectListener<TemperatureEffect>
{
    float attackTimer = 0;
    float distanceToPlayer;
    float patrolTimer = 0;
    Vector3 patrolCenter;
    GameObject player;
    [SerializeField] private float patrolRadius; //radius of which the enemy randomly moves while idle
    [SerializeField] private float patrolMoveSpeed;
    [SerializeField] private float patrolPositionChangeInterval;
    [SerializeField] private float chaseRadius;
    [SerializeField] private float chaseMoveSpeed;
    [SerializeField] private float backOffRadius;
    [SerializeField] private float backOffMoveSpeed;
    [SerializeField] private float attackRange; //range at which the enemy must be from the player to attack
    [SerializeField] private float attackInterval; //amount of time between each attack, will be randomized slighly.
    [SerializeField] private float moveSpeedDuringAtk;
    //[SerializeField] private GameObject projectileSpawnPos;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private float kbMultiplier;
    [SerializeField] private float kbDuration;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject fireVFX;
    [SerializeField] private float deathSequenceDuration;
    [SerializeField] public GameObject attackProjectile;
    public GameObject deathExplosion;
    Vector3 chaseOffset;
    Vector3 offsetVector;
    Vector3 diff;
    bool resetChaseOffset = true;
    bool hasBegunDeathSequence = false;
    bool isAttacking;


    void Start() {
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
        currHP = maxHP;
        playerDetector.OnPlayerEnter += OnPlayerEnter;
        agent.enabled = false;
    }
    public void OnSpawn(){
        agent.enabled = true;
    }

    void Death(){
        Invoke(nameof(invokeDeathEvent), 0.5f);
        GameObject g = Instantiate(deathExplosion, transform.position + new Vector3(0,0.5f,0), Quaternion.identity);
        g.GetComponent<NetworkObject>().Spawn();
        playerDetector.OnPlayerEnter -= OnPlayerEnter;
        Destroy(gameObject);
    }

    void StartDeathSequence(){
        //play animation, use animation events to determine speed;
        attackProjectile.GetComponent<FireSpriteProjectileController>().canAttack = false;
        EndAttack();
        hasBegunDeathSequence = true;
        agent.speed = chaseMoveSpeed * 1.2f;
        agent.stoppingDistance = 0;
        chaseRadius = 0.1f;
        backOffRadius = 0f;
        CancelInvoke();
        Invoke(nameof(Death), deathSequenceDuration);
        fireVFX.transform.localScale *= 1.5f;
    }
    
    public void OnEffect(TemperatureEffect effect)
    {
        if(effect.TempDelta < 0) { // an ice attack
            currHP -= Mathf.Abs(effect.TempDelta);
        }
        if (currHP <= 0 && !hasBegunDeathSequence) {
            StartDeathSequence();
        }
        UpdateHPBar();
    }

    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
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
            anim.SetBool("isMoving", false);
        } else {
            anim.SetBool("isMoving", true);
        }
    }

    void UpdateHPBar(){
        hpbarfill.GetComponent<Image>().fillAmount = currHP/maxHP;
    }

    void KnockBack(Vector3 dir){
        agent.enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * kbMultiplier, ForceMode.Impulse);
        Invoke("ResetKnockBack", kbDuration);
    }

    void ResetKnockBack(){
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.enabled = true;
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
            if (Time.time >= attackTimer && !hasBegunDeathSequence && !isAttacking) { //can attack
                SetAnimShoot();
            }
        }
    }

    void SetAnimShoot(){
        anim.SetBool("isAttacking", true);
    }

    public void ResetSpeed(){
        if(GetCurrentAggro() == null){
            agent.speed = patrolMoveSpeed;
        }else{
            agent.speed = chaseMoveSpeed;
        }
    }

    public void AttackPlayer(){
        isAttacking = true;
        agent.speed = chaseMoveSpeed / 2f;
        attackProjectile.GetComponent<FireSpriteProjectileController>().StartAttack();
        SlowSpeed();
    }

    public void EndAttack(){
        attackProjectile.GetComponent<FireSpriteProjectileController>().EndAttack();
        ResetSpeed();
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
        isAttacking = false;
        anim.SetBool("isAttacking", false);
    }

    public void SlowSpeed(){
        agent.speed = moveSpeedDuringAtk;
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
