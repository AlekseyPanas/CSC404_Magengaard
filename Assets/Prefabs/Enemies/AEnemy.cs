using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public abstract class AEnemy : NetworkBehaviour, IKillable {
    public event Action<GameObject> OnDeath;
    protected void invokeDeathEvent() { OnDeath(gameObject); }

    [SerializeField] protected float maxHP;
    [SerializeField] protected float currHP;
    [SerializeField] protected AAggroProvider aggroProvider;
    [SerializeField] protected NavMeshAgent agent;
    
    private GameObject _aggroTarget = null;
    private IKillable _aggroTargetKillable = null;
    private IAggroable _aggroTargetAggroable = null;
    private bool _hasSubscribedToAggroEvent;

    public void Start(){
        SubscribeToAggroEvent();
    }

    public void SubscribeToAggroEvent(){
        if (_hasSubscribedToAggroEvent) return;
        if (aggroProvider != null) {
            aggroProvider.AggroEvent += OnAggroTrigger;
        }
    }

    /*
        Can't directly add TryAggro to the AggroEvent because it returns a bool and I couldn't figure out how to make an event have a return type
    */
    void OnAggroTrigger(GameObject g){
        TryAggro(g);
    }

    /**
    * Try to switch the current aggro to a new game object. Return true if successful.
    * The target must implement IKillable for this to work. Also calls IAggroable method
    * if target implements the interface
    */
    protected bool TryAggro(GameObject g) {
        if (g == _aggroTarget) { return false; }
        var killable = g.GetComponent<IKillable>();
        if (killable == null) { return false; }

        OnNewAggro();
        if (_aggroTargetKillable != null) { _aggroTargetKillable.OnDeath -= _OnTargetDeath; }

        _aggroTarget = g;
        _aggroTargetKillable = killable;
        _aggroTargetKillable.OnDeath += _OnTargetDeath;

        var aggroable = g.GetComponent<IAggroable>();
        if (aggroable != null) {
            aggroable.Aggro(gameObject); 
            OnDeath += aggroable.DeAggro;
            _aggroTargetAggroable = aggroable;
        }

        return true;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if (_aggroTargetKillable != null) {
            _aggroTargetKillable.OnDeath -= _OnTargetDeath;
        }
    }

    private void _OnTargetDeath(GameObject gameObject) { DeAggroCurrent(); }

    /** 
    * Deaggro the current target if there is one set
    */
    protected void DeAggroCurrent() {
        if (_aggroTarget != null) {
            _aggroTargetKillable.OnDeath -= _OnTargetDeath;
            _aggroTarget = null;
            if (_aggroTargetAggroable != null) { _aggroTargetAggroable.DeAggro(gameObject); }
            OnDeAggro();
        }
    }

    /**
    * Return current aggro target, or null if no target is set
    */
    protected Transform GetCurrentAggro() { return _aggroTarget == null ? null : _aggroTarget.transform; }

    /**
    * Called internally when a deaggro occurs due to target's death or by any other means
    */
    protected virtual void OnDeAggro() {}

    /** 
    * Called whenever a new aggro target is set
    */
    protected virtual void OnNewAggro() {}

}
