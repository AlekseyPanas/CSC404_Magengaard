using System;
using Unity.Netcode;
using UnityEngine;

public abstract class AEnemy : NetworkBehaviour, IKillable {
    public event Action<GameObject> OnDeath;
    protected void invokeDeathEvent() { OnDeath(gameObject); }

    [SerializeField] protected float maxHP;
    [SerializeField] protected float currHP;
    
    private GameObject _aggroTarget = null;
    private IKillable _aggroTargetKillable = null;
    private IAggroable _aggroTargetAggroable = null;
    

    /**
    * Try to switch the current aggro to a new game object. Return true if successful.
    * The target must implement IKillable for this to work. Also calls IAggroable method
    * if target implements the interface
    */
    protected bool TryAggro(GameObject g) {
        var killable = g.GetComponent<IKillable>();
        if (killable == null) { return false; }

        if (_aggroTarget != g) { OnNewAggro(); }

        _aggroTarget = g;
        _aggroTargetKillable = killable;
        _aggroTargetKillable.OnDeath += _OnTargetDeath;

        var aggroable = g.GetComponent<IAggroable>();
        if (aggroable != null) {
            aggroable.Aggro(gameObject); 
            _aggroTargetAggroable = aggroable;
        }

        return true;
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
