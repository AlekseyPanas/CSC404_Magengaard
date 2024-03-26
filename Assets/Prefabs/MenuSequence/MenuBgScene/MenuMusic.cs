using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class MenuMusic : MonoBehaviour {

    private StudioEventEmitter _emitter;

    // Start is called before the first frame update
    void Start() {
        _emitter = GetComponent<StudioEventEmitter>();
        _emitter.AllowFadeout = true;
        _emitter.Play();
        MenuSceneChangeManager.OnSceneChange += OnSceneChange;
    }

    void OnSceneChange(string nextScene) {
        RuntimeManager.StudioSystem.setParameterByName("Out", 1);
    }
}
