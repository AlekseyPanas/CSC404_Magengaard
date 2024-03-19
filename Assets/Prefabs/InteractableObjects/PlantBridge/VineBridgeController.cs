using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VineBridgeController : NetworkBehaviour, IEffectListener<WaterEffect>
{
    [SerializeField] private List<GrowVine> vines;
    [SerializeField] float waterThreshold;
    [SerializeField] float timeToGrow;
    [SerializeField] GameObject bridgeCollider;
    Vector3 bridgeStartingScale;
    void Start(){
        bridgeStartingScale = bridgeCollider.transform.localScale;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            Debug.Log("growing");
            foreach(GrowVine g in vines){
                g.GrowVines();
                StartCoroutine(ExtendCollider());
            }
        }
    }
    public void OnEffect(WaterEffect effect)
    {
        if (effect.WaterVolume > waterThreshold){
            foreach(GrowVine g in vines){
                g.GrowVines();
                StartCoroutine(ExtendCollider());
            }
        }
    }

    IEnumerator ExtendCollider(){
        float timer = 0;
        while (timer < timeToGrow) {
            bridgeCollider.transform.localScale = Vector3.Lerp(Vector3.zero, bridgeStartingScale, timer/timeToGrow);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        bridgeCollider.transform.localEulerAngles = bridgeStartingScale;
    }
}
