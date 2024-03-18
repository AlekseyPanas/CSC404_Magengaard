using System.Collections;
using System.Collections.Generic;
using Parabox.CSG;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour, IEffectListener<TemperatureEffect> {

    [SerializeField] private GameObject _icePrefab;

    private GameObject _curIce = null;  // Tracks this water's ice object

    /** 
    * Reacting to cold effect
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta < 0) {

            try {
                // Compute intersection between water and the cold object
                var result = CSG.Intersect(gameObject, effect.Collider.gameObject);

                // Create new ice object and move it into position (this is to allow the dissolve animation to play)
                var newIce = Instantiate(_icePrefab);
                newIce.GetComponent<MeshFilter>().sharedMesh = result.mesh;
                newIce.transform.position = new Vector3(0, 0, 0);  // CSG offsets mesh for some reason such that 0,0,0 puts mesh in right spot
                
                // Once finished dissolving...
                newIce.GetComponent<Ice>().OnFinishedAppearing += () => {
                    if (_curIce == null) {
                        _curIce = newIce;

                        // If current ice fully melts, remove reference (since it will have destroyed itself)
                        _curIce.GetComponent<Ice>().OnFullyMelted += () => {
                            _curIce = null;
                        };

                    } else {
                        // Union the new ice into the current ice mesh and remove the other object
                        var union = CSG.Union(_curIce.gameObject, newIce);
                        _curIce.GetComponent<MeshFilter>().sharedMesh = union.mesh;
                        _curIce.transform.position = new Vector3(0, 0, 0);
                        Destroy(newIce);
                    }
                };
            } 
            catch {}  // Error thrown by CSG module if resulting mesh is empty (do nothing in that case)

        }
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
