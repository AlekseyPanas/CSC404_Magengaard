using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/** Network manager which hosts immediately */
public class StartHostNM : MonoBehaviour { 
    void Start() { GetComponent<NetworkManager>().StartHost(); }
}
