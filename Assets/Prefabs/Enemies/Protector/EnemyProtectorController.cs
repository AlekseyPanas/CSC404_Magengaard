using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

enum ATTACK_TYPE {
    SLASH = 1,
    DASH = 2,
    GROUND = 3
}

public class EnemyProtectorController : AEnemyAffectedByElement
{
    [SerializeField] private float damageNormalSlash;
    [SerializeField] private float damageDashAttack;
    [SerializeField] private float groundAttackRange;
    [SerializeField] private float chaseRadius;
    [SerializeField] private float chaseMoveSpeed;
    [SerializeField] private float backOffRadius;
    [SerializeField] private float backOffMoveSpeed;
    [SerializeField] private float attackRange; //range at which the enemy must be from the player to attack
    [SerializeField] private float attackInterval; //amount of time between each attack, will be randomized slighly.
    [SerializeField] private float moveSpeedDuringAtk;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private GameObject groundAttackProjectile;
    [SerializeField] private Transform raycastPosition;
    [SerializeField] private Transform groundAttackSpawnPosition;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashSpeed;
    [SerializeField] private Collider col;
    [SerializeField] private float rotationRate; // degrees per turn
    [SerializeField] private VisualEffect cape;
    [SerializeField] private VisualEffect soul;
    [SerializeField] private GameObject spellBarrier;
    float actionTimer = 0;
    float distanceToPlayer;
    GameObject player;
    Vector3 chaseOffset;
    Vector3 offsetVector;
    Vector3 diff;
    bool resetChaseOffset = true;
    bool isAttacking;
    List<GameObject> collided;
    float _damage;
    ATTACK_TYPE nextAttack;

    new void Start(){
        base.Start();
        agent.enabled = false;
        currHP = maxHP;
        chaseOffset = Vector3.zero;
        collided = new List<GameObject>();
        ChooseNextAttack();
        elementalResistances = new(){fire = 1, ice = 1, wind = 1, lightning = 1, impact = 0};
    }

    protected override void Death(){
        GetComponent<Collider>().enabled = false;
        rb.useGravity = false;
        _isAlive = false;
        anim.speed = 1;
        anim.Play("rig_Death");
        agent.enabled = false;
        hpbarCanvas.SetActive(false);
        cape.Stop();
        soul.Stop();
        spellBarrier.SetActive(false);
        Destroy(this);
    }

    void OnTriggerEnter(Collider col){
        if(col.CompareTag("Enemy")) return;
        if(!collided.Contains(col.gameObject)){
            IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect { Amount = (int)_damage, Direction = col.transform.position - transform.position });
            collided.Add(col.gameObject);
        }
    }

    public void ClearCollided(){
        collided.Clear();
    }
    void OnPlayerEnter(GameObject player) { TryAggro(player); }
    protected override void OnNewAggro() { Activate(); }

    public void OnActivate(){
        if(!_isAlive) return;
        agent.enabled = true;
        UpdateSpeed();
    }

    void Update()
    {
        if (!IsServer) return;
        if(agent.enabled){
            if (GetCurrentAggro() != null && !isAttacking) {
                ChasePlayer();
            }
        }
        hpbarCanvas.transform.LookAt(Camera.main.transform);
        anim.SetFloat("moveSpeed", agent.velocity.magnitude);
        if (agent.velocity.magnitude < 0.01f) {
            anim.SetBool("isMoving", false);
        } else {
            anim.SetBool("isMoving", true);
        }
    }

    protected override void UpdateHPBar(){
        hpbarfill.GetComponent<Image>().fillAmount = currHP/maxHP;
    }
    public void Activate(){
        _baseMoveSpeed = chaseMoveSpeed;
        UpdateSpeed();
        anim.SetTrigger("Activate");
    }
    void ChasePlayer(){
        LookAtPlayer();
        if (distanceToPlayer < groundAttackRange) {
            if(nextAttack == ATTACK_TYPE.GROUND) StartGroundAttackAnim();
        }

        if (distanceToPlayer > chaseRadius){ // need to move closer to player
            if(resetChaseOffset){
                resetChaseOffset = false;
                offsetVector = Vector3.Cross(diff, Vector3.up).normalized * Random.Range(-2f,2f);
                chaseOffset = (diff + offsetVector).normalized * chaseRadius;
            }
            MoveToPlayer();
        } else if (distanceToPlayer < backOffRadius) { // too close, need to back off
            BackOff(diff.normalized);
        }
        if (distanceToPlayer <= attackRange) { // can attack, need to check timer
            if (Time.time >= actionTimer && !isAttacking) { //can attack
                SwordAttack();
            }
        }
    }
    public void LookAtPlayer(){
        Vector3 target = GetCurrentAggro().position;
        diff = target - transform.position;
        distanceToPlayer = diff.magnitude;
        diff = new Vector3(diff.x, 0, diff.z);
        target = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(target, Vector3.up);
    }

    public void FacePlayer(){        
        diff = GetCurrentAggro().position - transform.position;
        distanceToPlayer = diff.magnitude;
        diff = new Vector3(diff.x, 0, diff.z);
        transform.forward = diff.normalized;
    }
    void StartGroundAttackAnim(){
        if(Time.time < actionTimer) return;
        FacePlayer();
        anim.SetTrigger("attack3");
        isAttacking = true;
        agent.enabled = false;
    }

    public void GroundAttack(){
        GameObject g = Instantiate(groundAttackProjectile, groundAttackSpawnPosition.position, Quaternion.identity);
        g.transform.forward = diff.normalized;
        g.GetComponent<GroundSlashController>().SetDirection(transform.forward);
        g.GetComponent<NetworkObject>().Spawn();
    }

    void SwordAttack(){
        FacePlayer();
        agent.enabled = false;
        isAttacking = true;
        if (nextAttack == ATTACK_TYPE.SLASH){
            _damage = damageNormalSlash;
            anim.SetTrigger("attack1");
        } else if (nextAttack == ATTACK_TYPE.DASH) {
            _damage = damageDashAttack;
            anim.SetTrigger("attack2");
        }
    }

    public void StartDash(){
        StartCoroutine(DashAttack());
    }

    public void EndAttack(){
        isAttacking = false;
        agent.enabled = true;
        actionTimer = Time.time + attackInterval * Random.Range(0.8f, 1.2f);
        ChooseNextAttack();
    }

    void ChooseNextAttack(){
        int i = Random.Range(0,10);
        if(i < 2){ // 20%
            nextAttack = ATTACK_TYPE.GROUND;
        }else if(i < 5){ // 40%
            nextAttack = ATTACK_TYPE.DASH;
        } else { // 40%
            nextAttack = ATTACK_TYPE.SLASH;
        }
    }

    IEnumerator DashAttack(){
        col.isTrigger = true;
        float timer = dashDuration;
        while(timer > 0){
            timer -= Time.deltaTime;
            rb.velocity = transform.forward * Mathf.Lerp(0, dashSpeed, timer / dashDuration);
            yield return new WaitForEndOfFrame();
        }
        rb.velocity = Vector3.zero;
        col.isTrigger = false;
    }

    void MoveToPlayer(){
        if(agent.enabled == false) return;
        if (Physics.Raycast(raycastPosition.position, diff, out var hit, Mathf.Infinity) && hit.transform.CompareTag("Ground")) {
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
        _baseMoveSpeed = backOffMoveSpeed;
        UpdateSpeed();
        agent.stoppingDistance = chaseRadius;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
