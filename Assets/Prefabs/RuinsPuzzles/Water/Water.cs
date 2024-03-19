using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour, IEffectListener<TemperatureEffect> {

    [SerializeField] private GameObject _icePrefab;
    [SerializeField] private float _yOffset = 0.1f;

    private List<Ice> _ices;  // Tracks this water's ice object

    void Start() {
        
    }

    /** 
    * Reacting to cold effect
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta < 0) {
            List<Vector2> verts2D = Const.MeshToFlatVertices(effect.mesh);
            
            foreach (var ice in _ices) {
                ice.collider.bounds.Contains(new Vector3(verts2D.x, ice.transform.position.y , verts2D.y));
            }

            Mesh m = Const.GetExtrudedConvexHullFromMeshProjection(, 0.1f);
            var newIce = Instantiate(_icePrefab);
            newIce.transform.position = transform.position;
            newIce.GetComponent<MeshFilter>().mesh = m;
            newIce.GetComponent<MeshCollider>().sharedMesh = m;
            newIce.transform.position = new Vector3(effect.mesh.transform.position.x, gameObject.transform.position.y + _yOffset, effect.mesh.transform.position.z);
        }
    }
}
