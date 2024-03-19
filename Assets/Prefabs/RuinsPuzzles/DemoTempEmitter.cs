using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTempEmitter : MonoBehaviour {

    public float tempDelta = 1;

    void Update() {
        //transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.01f);
    }

    void OnTriggerEnter(Collider col) {

        var temp = new TemperatureEffect {
            TempDelta = tempDelta,
            Collider = GetComponent<Collider>()
        };
        IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, temp);
    }
}
