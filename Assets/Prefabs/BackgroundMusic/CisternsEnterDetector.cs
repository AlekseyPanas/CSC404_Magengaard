using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CisternsEnterDetector : NetworkBehaviour {

    public static event Action OnEnterCisterns = delegate {};
    public static event Action OnExitCisterns = delegate {};

    [SerializeField] private PlayerDetector _playerDetectorFirst;
    [SerializeField] private PlayerDetector _playerDetectorSecond;

    void Awake() {
        _playerDetectorFirst.OnPlayerEnter += (GameObject player) => {
            OnEnterCisterns();
        };
        _playerDetectorSecond.OnPlayerEnter += (GameObject player) => {
            OnExitCisterns();
        };
    }
}
