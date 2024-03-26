using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

// list of resistances against each element from 0 to 1, 0 being no resistance and 1 being total resistance
public struct ElementalResistance{
    public float fire;
    public float ice;
    public float wind;
    public float lightning;
    public float impact;
}

public enum Element {
    fire = 1,
    ice = 2,
    wind = 3,
    lightning = 4,
    impact = 5
}

public abstract class AElementReactionManager : NetworkBehaviour, IEffectListener<TemperatureEffect>, IEffectListener<WindEffect>, IEffectListener<ImpactEffect>
{
    [SerializeField] protected AEnemyAffectedByElement _aEnemy;
    [SerializeField] protected ParticleSystem _slowParticles;
    [SerializeField] protected VisualEffect _burnParticles;
    [SerializeField] protected float _freezeThreshold;
    [SerializeField] protected float _burnThreshold;
    [SerializeField] protected float _internalTemperature;
    [SerializeField] protected float _tempResetRate;
    [SerializeField] protected float _kbDuration;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected float _burnInterval;
    [SerializeField] protected List<SkinnedMeshRenderer> _iceMeshes = new();

    protected bool _isBurning;
    protected bool _isFrozen;
    protected float _burnTimer = 0;


    public virtual void OnEffect(ImpactEffect effect){
        _aEnemy.TakeDamageWithElement(effect.Amount, Element.impact);
    }

    /*
    Update internal temperature
    */
    public virtual void OnEffect(TemperatureEffect effect)
    {
        if(effect.TempDelta > 0){
            _internalTemperature += effect.TempDelta;
            _aEnemy.TakeDamageWithElement(Mathf.Abs(effect.TempDelta), Element.fire);
        } else {
            _internalTemperature += effect.TempDelta;
            _aEnemy.TakeDamageWithElement(Mathf.Abs(effect.TempDelta), Element.ice);
        }
    }
    
    /**
    * Knockback effect from wind
    */
    public virtual void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity * effect.KBMultiplier);
        _aEnemy.TakeDamageWithElement(effect.Velocity.magnitude, Element.wind);
    }

    protected virtual void KnockBack(Vector3 dir){
        _aEnemy.GetAgent().enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * (1 - _aEnemy.GetWindResistance()), ForceMode.Impulse);
        Invoke(nameof(ResetKnockBack), _kbDuration);
    }

    protected virtual void ResetKnockBack(){ //if _stunDuration is 0, ResetStun will immediately be called and the agent will be reenabled
        _aEnemy.GetAgent().enabled = true;
    }

    protected virtual void Burn(){
        if(!_isBurning){
            _burnParticles.Play();
            _isBurning = true;
        }
        if(Time.time > _burnTimer){
            _aEnemy.TakeDamageWithElement(_internalTemperature * 0.01f, Element.fire);
            _burnTimer = Time.time + _burnInterval;
        }
    }

    protected virtual void Slow(){
        float dissolve = Mathf.Clamp((-_internalTemperature / _freezeThreshold * 1.5f) + 1, -1, 1);
        foreach(SkinnedMeshRenderer s in _iceMeshes){
            foreach(Material m in s.materials){
                m.SetFloat("_dissolve_amount", dissolve);
            }
        }
        _slowParticles.Play();
        _aEnemy.SetMoveSpeedModifier(Mathf.Clamp01(1 - ((1 - 1 / (-_internalTemperature)) * (1 - _aEnemy.GetIceResistance()))));
    }

    protected virtual void Freeze(){
        if(!_aEnemy.IsAlive()) return;
        
        foreach(SkinnedMeshRenderer s in _iceMeshes){
            foreach(Material m in s.materials){
                m.SetFloat("_dissolve_amount", 1);
            }
        }

        _isFrozen = true;
        _aEnemy.GetAgent().enabled = false;
        _aEnemy.PauseAnimator();
    }

    protected virtual void UnFreeze(){
        _isFrozen = false;
        _aEnemy.GetAgent().enabled = true;
        _aEnemy.ResumeAnimator();
    }

    protected virtual void Start(){
        _slowParticles.Stop();
        _burnParticles.Stop();
        Slow();
    }

    protected virtual void UpdateTemperature(){
        _slowParticles.Stop();
        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        }
        
        if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
            Slow();
        } else {
            _aEnemy.SetMoveSpeedModifier(1);
        }
        if(_internalTemperature > _burnThreshold){
            Burn();
            _isFrozen = false;
        } else {
            _burnParticles.Stop();
            _isBurning = false;
        }
        
        if (_internalTemperature < _freezeThreshold) {
            if(!_isFrozen) Freeze();
        }
        else{
            if(_isFrozen) UnFreeze();
        }
    }

    protected virtual void Update(){
        UpdateTemperature();
    }
}
