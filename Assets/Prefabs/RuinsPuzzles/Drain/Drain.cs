using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Drain : NetworkBehaviour {
    public event Action OnPlugged = delegate {};
    public event Action OnUnplugged = delegate {};
    public bool IsPlugged { get; private set; }

    private List<GameObject> _pluggers = new();

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    void OnTriggerEnter(Collider col) {
        _pluggers.Add(col.gameObject);

        if (_pluggers.Count == 1) {
            OnPlugged();
            IsPlugged = true;
        }
    }

    void OnTriggerExit(Collider col) {
        _pluggers.Remove(col.gameObject);

        if (_pluggers.Count == 0) {
            OnUnplugged();
            IsPlugged = false;
        }
    }
}
