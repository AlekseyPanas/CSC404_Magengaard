using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ice : NetworkBehaviour, IEffectListener<TemperatureEffect> {
    public event Action OnFinishedAppearing = delegate {};  // Fires when dissolve animation finishes
    public event Action OnFullyMelted = delegate {};  // Fires when ice is about to destroy itself because the mesh melted to 0

    [SerializeField] private float _timeToFade = 1f;

    [HideInInspector] public Collider collider;

    private string _shaderRefName = "_dissolve_amount";  // Reference to the dissolve parameter in the shader graph
    private float _maxDissolveAmount = 0.05f;
    private float _curDissolve;

    private Material _iceMat;

    // Start is called before the first frame update
    void Awake() {
        _curDissolve = _maxDissolveAmount;

        List<Material> mats = new();
        GetComponent<MeshRenderer>().GetMaterials(mats);
        _iceMat = mats[0];
        _iceMat.SetFloat(_shaderRefName, _curDissolve);

        collider = GetComponent<Collider>();

        StartCoroutine(FadeInIce());
    }

    /** Fade the ice in via the shader's dissolve effect. Fire finish event once done */
    private IEnumerator FadeInIce() {
        float startTime = Time.time;

        while (_curDissolve > 0) {
            float _percentFaded = (Time.time - startTime) / _timeToFade;

            _curDissolve = Mathf.Max(0, _maxDissolveAmount - (_maxDissolveAmount * _percentFaded));

            _iceMat.SetFloat(_shaderRefName, _curDissolve);

            yield return null;
        }

        OnFinishedAppearing();
        yield return null;
    }

    /** 
    * Intersection-based melting due to fire. Destroys self if fully melted
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta > 0) {
            
        }
    }
}
