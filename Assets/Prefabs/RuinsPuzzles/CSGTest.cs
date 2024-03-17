using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;

public class CSGTest : MonoBehaviour{

    public GameObject other;

    // Start is called before the first frame update
    void Start() {
        var result = CSG.Intersect(gameObject, other);
        GetComponent<MeshFilter>().sharedMesh = result.mesh;
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
