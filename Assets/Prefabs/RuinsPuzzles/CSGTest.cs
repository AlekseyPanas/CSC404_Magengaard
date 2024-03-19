using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using System;

public class CSGTest : MonoBehaviour{

    public GameObject[] others;

    // Start is called before the first frame update
    void Start() {
        // foreach (var o in others) {
        //     try {
        //         var result = CSG.Intersect(gameObject, o);
        //         GetComponent<MeshFilter>().mesh = result.mesh;
        //         transform.position = new Vector3(0, 0, 0);

        //     } catch (NullReferenceException e) {
        //         Debug.Log("YIKES! Im gone");
        //     }
        // }

        
        
    }

    public void subtract(GameObject other) {
        var result = CSG.Intersect(gameObject, other);
        GetComponent<MeshFilter>().mesh = result.mesh;
        transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
