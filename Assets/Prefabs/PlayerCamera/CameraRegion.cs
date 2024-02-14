using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRegion : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter, Horray! " + other.name);
        
        
    }
}
