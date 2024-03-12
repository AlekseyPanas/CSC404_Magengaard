using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySegment : MonoBehaviour
{
    [SerializeField] GameObject pivot;


    public void OnDeplete(){
        StartCoroutine(Shrink());
    }
    IEnumerator Shrink() {
        float timer = 0;
        float duration = 0.2f;
        while(timer < duration) {
            pivot.transform.localScale = new Vector3(Mathf.Lerp(1, 0, timer/duration), 1, 1);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }        
        Destroy(gameObject);
    }
}
