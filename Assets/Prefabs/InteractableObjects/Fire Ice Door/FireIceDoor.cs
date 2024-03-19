using System.Collections;
using UnityEngine;

public class FireIceDoor : ABarrierActivatable
{
    [SerializeField] MeshRenderer barrier;
    [SerializeField] Material matDissolve;
    [SerializeField] Collider col;

    new void Start(){
        base.Start();
        barrier.material = matDissolve;
        BarrierEnable();
    }

    protected override void BarrierDisable()
    {
        DeactivateBarrier();
    }

    protected override void BarrierEnable()
    {
        ActivateBarrier();
    }

    void ActivateBarrier(){
        col.enabled = true;
        matDissolve.SetFloat("_DissolveTime", -1);
    }

    void DeactivateBarrier(){
        StartCoroutine(DissolveBarrier(1f));
    }
    
    IEnumerator DissolveBarrier(float duration){
        matDissolve.SetFloat("_DissolveTime", -1);
        float timer = 0;
        while(timer < duration) {
            matDissolve.SetFloat("_DissolveTime", -1 + timer / duration * 2);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        matDissolve.SetFloat("_DissolveTime", 1);
        col.enabled = false;
    }
}
