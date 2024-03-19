using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProtectorController : AEnemy, IEffectListener<DamageEffect>, IEffectListener<WindEffect>
{
    [SerializeField] private float damageNormalSlash;
    [SerializeField] private float damageDashAttack;
    [SerializeField] private float chaseRadius;
    [SerializeField] private float chaseMoveSpeed;
    [SerializeField] private float backOffRadius;
    [SerializeField] private float backOffMoveSpeed;
    [SerializeField] private float attackRange; //range at which the enemy must be from the player to attack
    [SerializeField] private float attackInterval; //amount of time between each attack, will be randomized slighly.
    [SerializeField] private float moveSpeedDuringAtk;
    [SerializeField] private RectTransform hpbarfill;
    [SerializeField] private GameObject hpbarCanvas;
    [SerializeField] private float kbMultiplier;
    [SerializeField] private float kbDuration;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject groundAttackProjectile;
    [SerializeField] private float damageAngleThreshold;
    [SerializeField] private Transform raycastPosition;
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

    new void Start(){
        base.Start();
        agent.enabled = false;
        currHP = maxHP;
        chaseOffset = Vector3.zero;
        collided = new List<GameObject>();
    }

    void Death(){
        anim.Play("rig_Death");
        agent.enabled = false;
    }

    void OnTriggerEnter(Collider col){
        if(!collided.Contains(col.gameObject)){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)_damage, SourcePosition = transform.position });
            collided.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col){
        if(collided.Contains(col.gameObject)){
            collided.Remove(col.gameObject);
        }
    }

    public void OnEffect(DamageEffect effect)
    {
        Vector3 dir = transform.position - effect.SourcePosition;
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        float angle = Vector3.Angle(dir, transform.forward);
        Debug.Log("angle: " + angle);
        if (angle < damageAngleThreshold){
            currHP -= effect.Amount;
            if(currHP <= 0){
                Death();
            }
            UpdateHPBar();
        } else {
            //sparks
        }
    }

    public void OnEffect(WindEffect effect)
    {
        KnockBack(effect.Velocity);
    }
    void OnPlayerEnter(GameObject player) { TryAggro(player); }
    protected override void OnNewAggro() { SetChaseInfo(); }

    public void OnActivate(){
        agent.enabled = true;
    }

    void FixedUpdate()
    {
        if (!IsServer) return;
        if(agent.enabled){
            if (GetCurrentAggro() != null) {
                ChasePlayer();
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
        anim.SetTrigger("Activate");
    }
    void ChasePlayer(){
        diff = GetCurrentAggro().position - transform.position;
        distanceToPlayer = diff.magnitude;
        diff = new Vector3(diff.x, 0, diff.z);
        transform.forward = diff.normalized;
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
                TakeAction();
            }
        }
    }

    void TakeAction(){
        anim.Play("rig_SwingAttack01");
        agent.enabled = false;
        _damage = damageNormalSlash;
        isAttacking = true;
        //anim.SetTrigger("attack");
        //anim.SetBool("isAttacking", true);
    }

    public void EndAttack(){
        isAttacking = false;
        agent.enabled = true;
        actionTimer = Time.time + attackInterval;
    }

    public void ResetSpeed(){
        agent.speed = chaseMoveSpeed;
    }

    public void SlowSpeed(){
        agent.speed = moveSpeedDuringAtk;
    }

    void MoveToPlayer(){
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
        agent.speed = backOffMoveSpeed;
        agent.stoppingDistance = chaseRadius;
        agent.SetDestination(transform.position + (dir * -10f));
    }
}
