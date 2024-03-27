using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class BGMusic : MonoBehaviour {

    [SerializeField] private float _volumeScale;
    [SerializeField] private float _battleVolumeScale;

    private StudioEventEmitter _emitter;

    // Start is called before the first frame update
    void Start() {
        _emitter = GetComponent<StudioEventEmitter>();
        _emitter.EventInstance.setVolume(_volumeScale);
        DontDestroyOnLoad(gameObject);

        CisternsEnterDetector.OnEnterCisterns += () => { OnSceneChanged("Cisterns"); };
        CisternsEnterDetector.OnExitCisterns += () => { OnSceneChanged("Ruins"); };
        SceneTransition.OnSceneChanged += OnSceneChanged;
        PlayerCombatManager.OnEnterCombat += () => { OnToggleCombat(true); };
        PlayerCombatManager.OnExitCombat += () => { OnToggleCombat(false); };
    }

    void OnSceneChanged(string newScene) {
        if (newScene == "Ruins") {
            RuntimeManager.StudioSystem.setParameterByName("Progress", 1);
        } else if (newScene == "Cisterns") {
            RuntimeManager.StudioSystem.setParameterByName("Progress", 2);
        }
    }

    void OnToggleCombat(bool isInCombat) {
        if (isInCombat) {
            _emitter.EventInstance.setVolume(_battleVolumeScale);
            RuntimeManager.StudioSystem.setParameterByName("Combat", 1);
        } else {
            _emitter.EventInstance.setVolume(_volumeScale);
            RuntimeManager.StudioSystem.setParameterByName("Combat", 0);
        }
        
    }

    // Update is called once per frame
    void Update() {
        float val;
        RuntimeManager.StudioSystem.getParameterByName("Progress", out val);
    }
}
