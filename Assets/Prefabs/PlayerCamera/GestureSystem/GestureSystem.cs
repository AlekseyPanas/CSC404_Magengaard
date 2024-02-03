
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;

public class GestureSystem : MonoBehaviour, IGestureSystem
{
    public event GestureSuccess GestureSuccessEvent = _ => { };
    public event GestureBackfire GestureBackfire = _ => { };
    public event GestureFail GestureFail = () => { };
    
    private const float GESTURE_THRESHOLD = 0.5f;

    private bool _drawingEnabled = false;
    private List<Gesture> _recognizedGestures = new();
    
    [SerializeField] private GameObject trail;  // Trail object for gesture drawing
    [SerializeField] private GameObject particle_system;  // Particle system for gesture drawing (sparkles or smth)
    [SerializeField] private GameObject cam;  // Canvas camera used to place the particle system and trail in front of user
    [SerializeField] private LineRenderer line;  // Low alpha line persistent while drawing
    private List<Vector3> line_pts;
    private TrailRenderer trail_rend;  // The relevant component of the trail

    private List<Vector2> mouseTrack;  // List of user mouse points tracked during gesture drawing

    private static readonly float trail_collapse_factor_fast = 0.5f;  // How fast the trail vanishes while drawing
    private static readonly float trail_collapse_factor_slow = 0.05f;  // How fast the trail vanishes after releasing drawing
    private float trail_collapse_factor_cur;  // The current vanish rate
    
    // Start is called before the first frame update
    void Start() {
        mouseTrack = new List<Vector2>();
        line_pts = new List<Vector3>();
        trail_rend = trail.GetComponent<TrailRenderer>();
        trail_collapse_factor_cur = trail_collapse_factor_slow;
        line.SetPositions(line_pts.ToArray());
    }

    public void SetOverlayCameraStack(Camera playerCam){
        var cameraData = playerCam.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(cam.GetComponent<Camera>());
    }

    // Update is called once per frame
    void Update() {
        // Particle visuals
        Vector3 particlePos = cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(  // Get 3D position in front of camera
            Input.mousePosition.x, Input.mousePosition.y, 5));
        Vector3 linePos = cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(  // Get 3D position in front of camera
            Input.mousePosition.x, Input.mousePosition.y, 5.5f));
        trail.transform.position = particlePos;  // Move trail into position
        particle_system.transform.position = particlePos;  // Move particle system into postion
        MoveTrail();  // Collapse trail based on the collapse factor

        // Drawing gesture (mouse pressed)
        if (Input.GetKey(KeyCode.Mouse0)) {
            mouseTrack.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));  // Add user mouse point

            // Visual effects for drawing
            if (!trail_rend.emitting) {trail_rend.Clear();}  // Executed once at start of gesture drawing to remove any remaining trail points from old gestures
            trail_rend.emitting = true;
            trail_collapse_factor_cur = trail_collapse_factor_slow;
            var emission_module = particle_system.GetComponent<ParticleSystem>().emission;
            emission_module.enabled = true;
            line.gameObject.SetActive(true);  // Add line

            // Line renderer add point
            if (line_pts.Count == 0 || (line_pts[line_pts.Count - 1] - linePos).magnitude > 0.03) {
                line_pts.Add(linePos);
                line.positionCount = line_pts.Count;
                line.SetPositions(line_pts.ToArray());
            }
            
        } 
        
        // Mouse is released
        else {
            // If at least 10 user points accumulated, run gesture matching
            float minAcc = math.INFINITY;
            int index = -1;
            
            if (mouseTrack.Count > 10) {
                // Use Linq for minimum?
                for (int a = 0; a < _recognizedGestures.Count; a++)
                {
                    var gesture = _recognizedGestures[a];
                    
                    float acc = GestureUtils.compare_seq_to_gesture(
                        mouseTrack, gesture.Gest.ToList(), Const.NEXT_CHECKS, Const.MINIMIZATION_WEIGHTS, Const.FINAL_WEIGHTS, 0.01f);
                    
                    if (acc < minAcc){
                        minAcc = acc;
                        index = a;
                    }
                }
                
                Debug.Log("Gesture Accuracy: " + minAcc);
                
                if (minAcc < GESTURE_THRESHOLD) {
                    GestureSuccessEvent.Invoke(index);
                }
                else
                {
                    GestureFail.Invoke();
                }
            }
            else
            {
                GestureFail.Invoke();
            }
            
            mouseTrack = new List<Vector2>();  // Clear user points
            line_pts = new List<Vector3>();  // Clear line points
            line.gameObject.SetActive(false);  // Remove line

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

    public void enableGestureDrawing()
    {
        _drawingEnabled = true;
    }

    public void disableGestureDrawing()
    {
        _drawingEnabled = false;
    }

    public void setGesturesToRecognize(List<Gesture> gestures)
    {
        _recognizedGestures = gestures;
    }

    public bool isEnabled()
    {
        return _drawingEnabled;
    }

    public void clearGesturesToRecognize()
    {
        _recognizedGestures = new();
    }
}
