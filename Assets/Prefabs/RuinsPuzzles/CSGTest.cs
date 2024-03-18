using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using System;

public class CSGTest : MonoBehaviour{

    public GameObject[] others;

    // Start is called before the first frame update
    void Start() {
        foreach (var o in others) {
            try {
                var result = CSG.Union(gameObject, o);
                GetComponent<MeshFilter>().sharedMesh = result.mesh;
                transform.position = new Vector3(0, 0, 0);

            } catch (NullReferenceException e) {
                Debug.Log("YIKES! Im gone");
            }
        }

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
