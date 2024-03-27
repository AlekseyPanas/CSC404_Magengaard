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

    private bool isIn = false;

    void Awake() {
        _playerDetectorFirst.OnPlayerEnter += (GameObject player) => {
            if (isIn) {
                isIn = false;
                OnExitCisterns();
            }
        };
        _playerDetectorSecond.OnPlayerEnter += (GameObject player) => {
            if (!isIn) {
                OnEnterCisterns();
                isIn = true;
            }   
        };
    }
}
