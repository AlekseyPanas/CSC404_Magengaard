using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : MonoBehaviour
{

    public event Action OnFinishedAppearing = delegate {};  // Fires when dissolve animation finishes
    public event Action OnFullyMelted = delegate {};  // Fires when ice is about to destroy itself because the mesh melted to 0

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
