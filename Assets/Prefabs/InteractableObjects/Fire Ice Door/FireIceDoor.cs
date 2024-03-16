using System.Collections;
using UnityEngine;

public class FireIceDoor : ABarrierActivatable
{
    [SerializeField] MeshRenderer barrier;
    [SerializeField] Material matNormal;
    [SerializeField] Material matDissolve;
    [SerializeField] Collider col;
    protected override void BarrierDisable()
    {
        Debug.Log("Deactivating");
        DeactivateBarrier();
    }

    protected override void BarrierEnable()
    {
        ActivateBarrier();
    }

    void ActivateBarrier(){
        barrier.material = matNormal;
        col.enabled = true;
    }

    void DeactivateBarrier(){
        barrier.material = matDissolve;
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
