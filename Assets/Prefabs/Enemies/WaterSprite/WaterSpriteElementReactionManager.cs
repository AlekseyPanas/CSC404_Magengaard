using System;
using UnityEngine;

public class WaterSpriteElementReactionManager : AElementReactionManager 
{
    public MeshRenderer bubbleMesh;

    protected override void Start() {
        _slowParticles.Stop();
        Slow();
    }
    protected override void UpdateTemperature()
    {
        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        }
        
        if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
            Slow();
        } else {
            _aEnemy.SetMoveSpeedModifier(1);
        }
        
        if (_internalTemperature < _freezeThreshold) {
            if(!_isFrozen) Freeze();
        }
        else{
            if(_isFrozen) UnFreeze();
        }
    }

    protected override void Slow(){
        float dissolve = Mathf.Clamp((-_internalTemperature / _freezeThreshold * 1.5f) + 1, -1, 1);
        foreach(SkinnedMeshRenderer s in _iceMeshes){
            foreach(Material m in s.materials){
                m.SetFloat("_dissolve_amount", dissolve);
            }
        }
        foreach(Material m in bubbleMesh.materials){
            m.SetFloat("_dissolve_amount", dissolve);   
        }
        if (_internalTemperature < -_tempResetRate){
            _slowParticles.Play();
        }
        _aEnemy.SetMoveSpeedModifier(Mathf.Clamp01(1 - ((1 - 1 / (-_internalTemperature)) * (1 - _aEnemy.GetIceResistance()))));
    }

    protected override void Freeze(){
        if(!_aEnemy.IsAlive()) return;
        
        foreach(SkinnedMeshRenderer s in _iceMeshes){
            foreach(Material m in s.materials){
                m.SetFloat("_dissolve_amount", 1);
            }
        }
        foreach(Material m in bubbleMesh.materials){
            m.SetFloat("_dissolve_amount", 1);   
        }
        _isFrozen = true;
        _aEnemy.GetAgent().enabled = false;
        _aEnemy.PauseAnimator();
    }
}