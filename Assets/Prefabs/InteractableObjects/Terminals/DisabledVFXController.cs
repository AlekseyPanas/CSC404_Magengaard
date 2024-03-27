using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledVFXController : MonoBehaviour
{
    [SerializeField] float rotateSpeed;
    [SerializeField] List<LineRenderer> lr = new();
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    IEnumerator Enable(){
        float t = 0;
        float d = 1f;
        while (t < d) {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t/d);
            foreach(LineRenderer l in lr){
                Color c = l.material.color;
                Color newC = new (c.r, c.g, c.b, alpha);
                l.material.color = newC;
            }
            yield return new WaitForEndOfFrame();
        }
    } 

    IEnumerator Disable(){
        float t = 0;
        float d = 1f;
        while (t < d) {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t/d);
            foreach(LineRenderer l in lr){
                Color c = l.material.color;
                Color newC = new (c.r, c.g, c.b, alpha);
                l.material.color = newC;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void EnableVFX(){
        StartCoroutine(Enable());
    }

    public void DisableVFX(){
        StartCoroutine(Disable());
    }
}
