using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour, IEffectListener<TemperatureEffect> {

    [SerializeField] private GameObject _icePrefab;

    private List<GameObject> _ices;  // Tracks this water's ice object

    private bool b = false;

    void Start() {
        
    }

    /** 
    * Reacting to cold effect
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta < 0 && !b) {

            Mesh m = Const.GetExtrudedConvexHullFromMeshProjection(Const.MeshToFlatVertices(effect.mesh), 0.1f);
            var newIce = Instantiate(_icePrefab);
            newIce.transform.position = transform.position;
            newIce.GetComponent<MeshFilter>().mesh = m;
            newIce.GetComponent<MeshCollider>().sharedMesh = m;
            newIce.transform.position = new Vector3(effect.mesh.transform.position.x, gameObject.transform.position.y, effect.mesh.transform.position.z);

            b = true;
        }
    }
}
