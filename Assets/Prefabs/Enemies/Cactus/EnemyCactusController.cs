using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyCactusController : NetworkBehaviour, IEffectListener<DamageEffect>, IEffectListener<WindEffect>, IEnemy
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
    [SerializeField] private Transform projectileSpawnPos;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private float kbMultiplier;
    [SerializeField] private float kbDuration;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private PlayerDetector playerDetector;
    public bool canAgro = false;
    public GameObject attackProjectile;
    public event Action<GameObject> OnEnemyDeath;

    public void OnDeath(){
        OnEnemyDeath?.Invoke(gameObject);
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    
    public void OnEffect(DamageEffect effect)
    {
        currHP -= effect.Amount;
        if (currHP <= 0) {
            OnDeath();
        }
        UpdateHPBar();
    }

    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
    }

    public void OnPlayerEnter(){
        canAgro = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        target = null;
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
        currHP = maxHP;
        playerDetector.OnPlayerEnter += OnPlayerEnter;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;
        if(agent.enabled){
            if(canAgro) {
                player = GameObject.FindWithTag("Player");
                target = player;
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
        if (agent.velocity == Vector3.zero) {
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
        agent.stoppingDistance = chaseRadius;
        agent.angularSpeed = 0;
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
        Vector3 diff = target.transform.position - transform.position;
        distanceToPlayer = diff.magnitude;
        transform.forward = new Vector3(diff.x, 0, diff.z).normalized; //always faces the player when chasing
        if (distanceToPlayer > chaseRadius){ // need to move closer to player
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
        float intervalRandomizer = UnityEngine.Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
        GameObject proj = Instantiate(attackProjectile, projectileSpawnPos.position, Quaternion.identity); //projectile behaviour will be handled on the projectile object
        Vector3 shootDir = target.transform.position - transform.position;
        shootDir = new Vector3(shootDir.x, 0, shootDir.z).normalized;
        proj.GetComponent<EnemyCactusProjectileController>().SetTargetDirection(shootDir);
        proj.GetComponent<NetworkObject>().Spawn();
    }

    public void SlowSpeed(){
        if(target == null){
            agent.speed = 0.1f;
        }else{
            agent.speed = 0.1f;
        }
    }

    void MoveToPlayer(){
        agent.SetDestination(target.transform.position);
    }

    void BackOff(Vector3 dir){
        agent.speed = backOffMoveSpeed;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
