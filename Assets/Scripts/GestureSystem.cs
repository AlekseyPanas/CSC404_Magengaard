
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureSystem : MonoBehaviour
{

    [SerializeField] private GameObject trail;
    [SerializeField] private GameObject particle_system;
    [SerializeField] private GameObject cam;
    private TrailRenderer trail_rend;

    List<Vector2> mouseTrack;
    private float trail_collapse_factor_fast = 0.5f;
    private float trail_collapse_factor_slow = 0.05f;
    private float trail_collapse_factor_cur = 0f;

    // Start is called before the first frame update
    void Start() {
        mouseTrack = new List<Vector2>();
        trail_rend = trail.GetComponent<TrailRenderer>();
        trail_collapse_factor_cur = trail_collapse_factor_slow;
    }

    // Update is called once per frame
    void Update() {
        // Particle visuals
        Vector3 particlePos = cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, Input.mousePosition.y, 5));
        trail.transform.position = particlePos;
        particle_system.transform.position = particlePos;
        MoveTrail();


        if (Input.GetKey(KeyCode.Mouse0)) {
            mouseTrack.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            trail_rend.emitting = true;
            trail_collapse_factor_cur = trail_collapse_factor_slow;
        } else {
            if (mouseTrack.Count > 10) {
                float acc = GestureUtils.compare_seq_to_gesture(mouseTrack, Const.G1, Const.NEXT_CHECKS, Const.MINIMIZATION_WEIGHTS, Const.FINAL_WEIGHTS, 0.01f);
                Debug.Log(acc);
            }
            mouseTrack = new List<Vector2>();
            trail_rend.emitting = false;
            trail_collapse_factor_cur = trail_collapse_factor_fast;
        }
    }

    void MoveTrail() {
        Vector3[] poses = new Vector3[1000];
        int num_particles = trail_rend.GetPositions(poses);
        int skip = 2;
        for (int i = num_particles - (1 + skip); i >= 0; i--) {
            Vector3 delta = (poses[i + skip] - poses[i]) * trail_collapse_factor_cur;
            trail_rend.SetPosition(i, poses[i] + delta);
        }
    }
}
