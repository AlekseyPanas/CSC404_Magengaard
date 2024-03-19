using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour, IEffectListener<TemperatureEffect> {

    [SerializeField] private GameObject _icePrefab;

    public GameObject redBall;

    private GameObject _curIce;  // Tracks this water's ice object

    private bool b = false;

    void Start() {
        // Mesh m = Const.GetExtrudedConvexHullFromMeshProjection(GetComponent<MeshFilter>().mesh, 0.5f);
        // var newIce = Instantiate(_icePrefab);
        // newIce.transform.position = transform.position;
        // newIce.GetComponent<MeshFilter>().mesh = m;
        // newIce.GetComponent<MeshCollider>().sharedMesh = m;

        foreach (var v in Const.GetExtrudedConvexHullFromMeshProjection(gameObject, 0.5f)) {
            var obj = Instantiate(redBall);
            obj.transform.position = new Vector3(v.x, transform.position.y + 3, v.y);
        }
    }

    /** 
    * Reacting to cold effect
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta < 0 && !b) {

            

            b = true;
        }
    }
}
