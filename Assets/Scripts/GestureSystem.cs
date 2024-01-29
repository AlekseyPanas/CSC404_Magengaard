
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureSystem : MonoBehaviour
{

    [SerializeField] private GameObject trail;  // Trail object for gesture drawing
    [SerializeField] private GameObject particle_system;  // Particle system for gesture drawing (sparkles or smth)
    [SerializeField] private GameObject cam;  // Main camera used to place the particle system and trail in front of user
    private TrailRenderer trail_rend;  // The relevant component of the trail

    private List<Vector2> mouseTrack;  // List of user mouse points tracked during gesture drawing

    private static readonly float trail_collapse_factor_fast = 0.5f;  // How fast the trail vanishes while drawing
    private static readonly float trail_collapse_factor_slow = 0.05f;  // How fast the trail vanishes after releasing drawing
    private float trail_collapse_factor_cur;  // The current vanish rate

    // Start is called before the first frame update
    void Start() {
        mouseTrack = new List<Vector2>();
        trail_rend = trail.GetComponent<TrailRenderer>();
        trail_collapse_factor_cur = trail_collapse_factor_slow;
    }

    // Update is called once per frame
    void Update() {
        // Particle visuals
        Vector3 particlePos = cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(  // Get 3D position in front of camera
            Input.mousePosition.x, Input.mousePosition.y, 5));
        trail.transform.position = particlePos;  // Move trail into position
        particle_system.transform.position = particlePos;  // Move particle system into postion
        MoveTrail();  // Collapse trail based on the collapse factor

        // Drawing gesture (mouse pressed)
        if (Input.GetKey(KeyCode.Mouse0)) {
            mouseTrack.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));  // Add user mouse point

            // Enable visual effects for drawing
            if (!trail_rend.emitting) {trail_rend.Clear();}  // Executed once at start of gesture drawing to remove any remaining trail points from old gestures
            trail_rend.emitting = true;
            trail_collapse_factor_cur = trail_collapse_factor_slow;
            var emission_module = particle_system.GetComponent<ParticleSystem>().emission;
            emission_module.enabled = true;
        } 
        
        // Mouse is released
        else {
            // If at least 10 user points accumulated, run gesture matching
            if (mouseTrack.Count > 10) {
                float acc = GestureUtils.compare_seq_to_gesture(mouseTrack, Const.G1, Const.NEXT_CHECKS, Const.MINIMIZATION_WEIGHTS, Const.FINAL_WEIGHTS, 0.01f);
                Debug.Log("Gesture Accuracy: " + acc);
            }
            mouseTrack = new List<Vector2>();  // Clear user points

            // Disable visual effects
            trail_rend.emitting = false;
            trail_collapse_factor_cur = trail_collapse_factor_fast;
            var emission_module = particle_system.GetComponent<ParticleSystem>().emission;  
            emission_module.enabled = false;
        }
    }

    /**
     * Collapses the trail constantly to avoid the "lag" effect when moving mouse in a jittery manner
     */
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
