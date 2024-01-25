using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SandstormScript : NetworkBehaviour
{

    [SerializeField] private Transform sandstormPrefab;

    public static Transform CreateSandstorm() {
        //Instantiate()
        Instantiate(sandstormPrefab);
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
