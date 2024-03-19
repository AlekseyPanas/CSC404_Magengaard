using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour, IEffectListener<TemperatureEffect> {

    [SerializeField] private GameObject _whirlpoolPrefab;
    [SerializeField] private GameObject _icePrefab;
    [SerializeField] private float _yOffset = 0.1f;
    [SerializeField] private bool _isDeadly = true;
    [SerializeField] private List<Drain> _drains;  // Optionally provide drains. This enables the height control
    [SerializeField] Transform _maxHeightIndicator;  // Water doesn't go higher than this
    [SerializeField] float _waterRiseSpeed = 0f;

    private ParticleSystem[] _whirlpools;
    //public GameObject redBall;

    private List<Tuple<Ice, List<Vector2>>> _ices = new();  // Tracks this water's ice object

    private Dictionary<Ice, List<Ice>> _garbage = new();

    // Prevents freezing in same frame which bugs out the merging system
    private List<TemperatureEffect> _queue = new();
    private bool _isLocked = false;

    private Collider _collider;

    void Start() {
        _collider = GetComponent<Collider>();

        _whirlpools = new ParticleSystem[_drains.Count];
        for (int d = 0; d < _drains.Count; d++) {
            var obj = Instantiate(_whirlpoolPrefab);
            obj.transform.position = new Vector3(_drains[d].transform.position.x, _drains[d].transform.position.y + _yOffset, _drains[d].transform.position.z);
            _whirlpools[d] = obj.GetComponent<ParticleSystem>();
            var emission_module = _whirlpools[d].emission;
            emission_module.enabled = false;
        }
    }

    /** 
    * Currently water instakills player (unless not isDeadly)
    */
    void OnTriggerEnter(Collider other) {
        if (!_isDeadly) { return; }

        if (other.gameObject.tag == "Player") {
            IEffectListener<DamageEffect>.SendEffect(other.gameObject, new DamageEffect(){Amount = 1000, SourcePosition = transform.position});
        }
    }

    void Update() {
        if (!_isLocked && _queue.Count > 0) {
            OnEffect(_queue[0]);
            _queue.RemoveAt(0);
        }

        _isLocked = false;
    }

    /** 
    * Reacting to cold effect
    */
    public void OnEffect(TemperatureEffect effect) {
        if (!IsServer) { return; }

        if (effect.TempDelta < 0) {

            // If locked, enqueue, otherwise take lock and go
            if (_isLocked) {
                _queue.Add(effect);
                return;
            }
            _isLocked = true;

            // Gets flattened vertices of new ice
            List<Vector2> verts2D = Const.MeshToFlatVertices(effect.mesh);
            
            // Collects all existing ice tuples whose bounds encompass at least one flattened vertex
            List<Tuple<Ice, List<Vector2>>> collidedIces = new();
            foreach (var tup in _ices) {
                foreach (var pt in verts2D) {
                    var pos = new Vector3(effect.mesh.transform.position.x + pt.x, 
                                                                    tup.Item1.transform.position.y , 
                                                                    effect.mesh.transform.position.z + pt.y);
                    //var ball = Instantiate(redBall);
                    //ball.transform.position = pos;
                    if (tup.Item1.collider.bounds.Contains(pos)) {
                        collidedIces.Add(tup);
                        break;
                    }
                }
            }

            // Merges flattened vertices of collided ices with this one
            foreach (var tup in collidedIces) {
                Vector3 diff3D = tup.Item1.transform.position - effect.mesh.transform.position;
                Vector2 diff2D = new Vector2(diff3D.x, diff3D.z);
                foreach (var vec in tup.Item2) { verts2D.Add(diff2D + vec); }
            }

            // Creates new ice object using flattened vertices
            var hullTup = Const.GetExtrudedConvexHullFromMeshProjection(verts2D, 0.1f);
            Mesh m = hullTup.Item1;
            var newIce = Instantiate(_icePrefab);
            newIce.transform.position = transform.position;
            newIce.GetComponent<MeshFilter>().mesh = m;
            newIce.GetComponent<MeshCollider>().sharedMesh = m;
            newIce.transform.position = new Vector3(effect.mesh.transform.position.x, gameObject.transform.position.y + _yOffset, effect.mesh.transform.position.z);
            Ice pureIce = newIce.GetComponent<Ice>();

            // Add new ice object to main list
            _ices.Add(new Tuple<Ice, List<Vector2>>(pureIce, hullTup.Item2));

            List<Ice> colliderJustIces = new();
            foreach (var tup in collidedIces) { 
                colliderJustIces.Add(tup.Item1); 
                _ices.Remove(tup);  // Remove all collided ices from main list
            }
            // Add collided ices to garbage dict
            _garbage.Add(pureIce, colliderJustIces);

            // Clears garbage once fully appeared
            pureIce.OnFinishedAppearing += () => {
                if (pureIce.isFinishedAppearing) { return; }
                if (!_garbage.ContainsKey(pureIce)) { return; }
                foreach (var ic in _garbage[pureIce]) {
                    ic.OnFinishedAppearing();
                    Destroy(ic.gameObject);
                }
                _garbage.Remove(pureIce);
            };
        }
    }
}

// TODO: Despite all efforts, it appears that sometimes ice separates into multiple objects despite
//  that it should be merged. Could be an update timing issue, idk
