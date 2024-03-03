using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyFireSpriteController : NetworkBehaviour, IEffectListener<WindEffect>, IEffectListener<TemperatureEffect>, IEnemy
{
    GameObject target;
    float attackTimer = 0;
    float distanceToPlayer;
    float patrolTimer = 0;
    NavMeshAgent agent;
    Vector3 patrolCenter;
    GameObject player;
    [SerializeField] private float maxHP;
    [SerializeField] private float currHP;
    [SerializeField] private float patrolRadius; //radius of which the enemy randomly moves while idle
    [SerializeField] private float patrolMoveSpeed;
    [SerializeField] private float patrolPositionChangeInterval;
    [SerializeField] private float chaseRadius;
    [SerializeField] private float chaseMoveSpeed;
    [SerializeField] private float backOffRadius;
    [SerializeField] private float backOffMoveSpeed;
    [SerializeField] private float attackRange; //range at which the enemy must be from the player to attack
    [SerializeField] private float attackInterval; //amount of time between each attack, will be randomized slighly.
    [SerializeField] private float attackDuration;
    [SerializeField] private GameObject projectileSpawnPos;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private float kbMultiplier;
    [SerializeField] private float kbDuration;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private float deathSequenceDuration;
    public PlayerDetector playerDetector;
    public bool canAgro = false;
    public GameObject attackProjectile;
    public GameObject deathExplosion;
    public event Action<GameObject> OnEnemyDeath;
    Vector3 chaseOffset;
    Vector3 offsetVector;
    Vector3 diff;
    bool resetChaseOffset = true;
    GameObject spawnedProjectile;
    bool hasBegunDeathSequence = false;

    void OnDeath(){
        OnEnemyDeath?.Invoke(gameObject);
        GameObject g = Instantiate(deathExplosion, transform.position, Quaternion.identity);
        g.GetComponent<NetworkObject>().Spawn();
        Destroy(spawnedProjectile);
        Destroy(gameObject);
        playerDetector.OnPlayerEnter -= OnPlayerEnter;
        PlayerHealthSystem.onDeath -= ResetAgro;
    }

    void StartDeathSequence(){
        //play animation, use animation events to determine speed;
        hasBegunDeathSequence = true;
        agent.speed = chaseMoveSpeed * 2f;
        agent.stoppingDistance = 0;
        chaseRadius = 0.1f;
        backOffRadius = 0f;
        Destroy(spawnedProjectile);
        hasBegunDeathSequence = true;
        CancelInvoke();
        Invoke(nameof(OnDeath), deathSequenceDuration);
    }
    
    public void OnEffect(TemperatureEffect effect)
    {
        if(effect.TempDelta < 0) { // an ice attack
            currHP -= Mathf.Abs(effect.TempDelta);
        }
        if (currHP <= 0) {
            StartDeathSequence();
        }
        UpdateHPBar();
    }

    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
    }

    void OnPlayerEnter(GameObject player){
        canAgro = true;
        target = player;
    }
    void ResetAgro(){
        canAgro = false;
        agent.speed = patrolMoveSpeed;
        target = null;
    }

    void Start()
    {
        target = null;
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
        currHP = maxHP;
        playerDetector.OnPlayerEnter += OnPlayerEnter;
        PlayerHealthSystem.onDeath += ResetAgro;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;
        if(agent.enabled){
            if(canAgro) {
                canAgro = false; // to prevent it from searching for the player again
                SetChaseInfo();
            }
            if (target != null) {
                ChasePlayer();
            } else {
                Patrol();
            }
        }
        hpbarCanvas.transform.LookAt(Camera.main.transform);
        // animation stuff
        // if (agent.velocity.magnitude < 0.05f) {
        //     anim.SetBool("isMoving", false);
        // } else {
        //     anim.SetBool("isMoving", true);
        // }
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
        diff = target.transform.position - transform.position;
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
            if (Time.time >= attackTimer && !hasBegunDeathSequence) { //can attack
                //SetAnimShoot();
                AttackPlayer();
            }
        }
    }

    void SetAnimShoot(){
        anim.SetBool("isShooting", true);
    }

    public void ResetSpeed(){
        if(target == null){
            agent.speed = patrolMoveSpeed;
        }else{
            agent.speed = chaseMoveSpeed;
        }
    }

    public void AttackPlayer(){
        agent.speed = chaseMoveSpeed / 2f;
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
        spawnedProjectile = Instantiate(attackProjectile); //projectile behaviour will be handled on the projectile object
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        spawnedProjectile.GetComponent<FireSpriteProjectileController>().parent = projectileSpawnPos;
        spawnedProjectile.GetComponent<FireSpriteProjectileController>().lifetime = attackDuration;
        Invoke(nameof(ResetSpeed), attackDuration);
    }

    public void SlowSpeed(){
        agent.speed = 0.1f;
    }

    void MoveToPlayer(){
        if (Physics.Raycast(transform.position, diff, out var hit, Mathf.Infinity) && hit.transform.CompareTag("Ground")) {
            resetChaseOffset = true;
            agent.stoppingDistance = chaseRadius;
            agent.SetDestination(target.transform.position);
        } else {
            // if nothing is blocking
            agent.stoppingDistance = 0;
            agent.SetDestination(target.transform.position - chaseOffset); //i have no idea why chaseOffset has to be subtracted here. if it is added, the offset goes past the player
        }
    }

    void BackOff(Vector3 dir){
        resetChaseOffset = true;
        agent.speed = backOffMoveSpeed;
        agent.stoppingDistance = chaseRadius;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
