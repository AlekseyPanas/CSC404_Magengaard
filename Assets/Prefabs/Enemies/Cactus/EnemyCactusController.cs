using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyCactusController : NetworkBehaviour, IEffectListener<DamageEffect>
{
    float maxHP;
    float currHp;
    float damageToTake;
    GameObject target;
    Rigidbody rb;
    Vector3 moveDir;
    Collider[] colsInRange;
    float attackTimer = 0;
    float distanceToPlayer;
    float patrolTimer = 0;
    NavMeshAgent agent;
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
    public bool canAgro = false;
    public GameObject attackProjectile;
    
    public void OnEffect(DamageEffect effect)
    {
        damageToTake = effect.Amount;
    }

    // Start is called before the first frame update
    void Start()
    {
        target = null;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;
        agent.speed = patrolMoveSpeed;    
        agent.stoppingDistance = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if(canAgro) {
            target = GameObject.FindWithTag("Player");
            canAgro = false; // to prevent it from searching for the player again
            SetChaseInfo();
        }

        if (target != null) {
            ChasePlayer();
        } else {
            Patrol();
        }
    }

    void SetChaseInfo(){
        agent.speed = chaseMoveSpeed;    
        agent.stoppingDistance = chaseRadius;
        agent.angularSpeed = 0;
    }

    void Patrol(){
        if(Time.time > patrolTimer){
            float r = Random.Range(0f,1f);
            if (r < 0.7f){
                float randX = Random.Range(-patrolRadius, patrolRadius);
                float randZ = Random.Range(-patrolRadius, patrolRadius);
                agent.SetDestination(patrolCenter + new Vector3(randX, 0, randZ));
            }
            patrolTimer = Time.time + patrolPositionChangeInterval * Random.Range(-0.5f, 1.5f);
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
                AttackPlayer(diff.normalized);
            }
        }
    }

    void AttackPlayer(Vector3 dir){
        Debug.Log("attacking");
        float intervalRandomizer = Random.Range(0.8f, 1.2f);
        attackTimer = Time.time + attackInterval * intervalRandomizer;
        GameObject proj = Instantiate(attackProjectile, projectileSpawnPos.position, Quaternion.identity); //projectile behaviour will be handled on the projectile object
        proj.GetComponent<EnemyCactusProjectileController>().SetTargetDirection(dir);
        proj.GetComponent<NetworkObject>().Spawn();
    }

    void MoveToPlayer(){
        agent.speed = chaseMoveSpeed;
        agent.SetDestination(target.transform.position);
    }

    void BackOff(Vector3 dir){
        agent.speed = backOffMoveSpeed;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
