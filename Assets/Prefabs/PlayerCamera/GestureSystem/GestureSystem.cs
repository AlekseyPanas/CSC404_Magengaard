
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class GestureSystem : AGestureSystem
{
    private static readonly float TRAIL_COLLAPSE_FACTOR_FAST = 0.5f;  // How fast the trail vanishes while drawing
    private static readonly float TRAIL_COLLAPSE_FACTOR_SLOW = 0.05f;  // How fast the trail vanishes after releasing drawing
    private static readonly float DRAG_DIST_TO_ADD = 0.005f;  // When dragging, adds a mousepoint only if it is at least this distance away from the previous one as a percentage of the screen size
    private static readonly float MIN_GEST_DRAG_DIST = 0.17f;  // Distance to drag to be considered a valid gesture. Measured as percentage of screen w/h where max diagonal distance amounts to 1.41

    private bool _drawingEnabled = false;
    private List<Gesture> _recognizedGestures = new();
    
    [SerializeField] private GameObject trail;  // Trail object for gesture drawing
    [SerializeField] private GameObject particle_system;  // Particle system for gesture drawing (sparkles or smth)
    [SerializeField] private GameObject cam;  // Canvas camera used to place the particle system and trail in front of user
    [SerializeField] private LineRenderer line;  // Low alpha line persistent while drawing
    private List<Vector3> line_pts;
    private TrailRenderer trail_rend;  // The relevant component of the trail

    private List<Vector2> mouseTrack;  // List of user mouse points tracked during gesture drawing
    private float cum_dist = 0;  // Cumilative distance between mouse points as a percentage of screen size(used as gesture threshold)
    bool began_drawing_event_sent = false;

    private float trail_collapse_factor_cur;  // The current vanish rate

    private DesktopControls _controls;
    
    // Start is called before the first frame update
    void Start() {
        mouseTrack = new List<Vector2>();
        line_pts = new List<Vector3>();
        trail_rend = trail.GetComponent<TrailRenderer>();
        trail_collapse_factor_cur = TRAIL_COLLAPSE_FACTOR_SLOW;
        line.SetPositions(line_pts.ToArray());
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
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
        if (_controls.Game.Fire.IsPressed()) {
            Vector2 new_mouse_pos = _controls.Game.MousePos.ReadValue<Vector2>();
            Vector2 scaled_new_mouse_pos = new Vector2(new_mouse_pos.x / Screen.width, new_mouse_pos.y / Screen.height);
            Vector2 scaled_former_mouse_pos = mouseTrack.Count > 0 ? new Vector2(mouseTrack[mouseTrack.Count - 1].x / Screen.width, mouseTrack[mouseTrack.Count - 1].y / Screen.height): scaled_new_mouse_pos;
            float diff_mag = (scaled_former_mouse_pos - scaled_new_mouse_pos).magnitude;
            if (diff_mag > DRAG_DIST_TO_ADD || mouseTrack.Count == 0) {
                mouseTrack.Add(new_mouse_pos);  // Add user mouse point
                cum_dist += diff_mag;
            } 
            // Sends drawing event after a certain distance. Doesn't send if no gestures set since in that case, no gesture would be recognized
            if (cum_dist > MIN_GEST_DRAG_DIST && !began_drawing_event_sent && _recognizedGestures.Count > 0) { invokeBeganDrawingEvent(); began_drawing_event_sent = true; }

            // Visual effects for drawing
            if (!trail_rend.emitting) {trail_rend.Clear();}  // Executed once at start of gesture drawing to remove any remaining trail points from old gestures
            trail_rend.emitting = true;
            trail_collapse_factor_cur = TRAIL_COLLAPSE_FACTOR_SLOW;
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
            // Only match if past length threshold
            if (cum_dist > MIN_GEST_DRAG_DIST) {

                // Find best matched gesture of those provided
                float minAcc = math.INFINITY;
                int index = -1;

                for (int a = 0; a < _recognizedGestures.Count; a++) {
                    var gesture = _recognizedGestures[a];
                    float acc = GestureUtils.compare_seq_to_gesture(mouseTrack, gesture.Gest.ToList(), Const.NEXT_CHECKS, Const.MINIMIZATION_WEIGHTS, Const.FINAL_WEIGHTS, 0.01f);
                    if (acc < minAcc) { minAcc = acc; index = a; }
                }
                
                //Debug.Log("Gesture Accuracy: " + minAcc);
                
                // Don't fire any events if there are no gestures set to recognize
                if (index != -1) {
                    // Fire appropriate event
                    if (minAcc <= _recognizedGestures[index].SuccessAccuracy) { invokeGestureSuccessEvent(index); } 
                    else if (minAcc <= _recognizedGestures[index].BackfireFailAccuracy) { invokeGestureBackfireEvent(index); }
                    else { invokeGestureFailEvent(); }
                }
            }

            mouseTrack = new List<Vector2>();  // Clear user points
            cum_dist = 0f;  // Clears cumilative distance
            began_drawing_event_sent = false;
            
            // Disable visual effects
            line_pts = new List<Vector3>();  // Clear line points
            line.gameObject.SetActive(false);  // Remove line
            trail_rend.emitting = false;
            trail_collapse_factor_cur = TRAIL_COLLAPSE_FACTOR_FAST;
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

    public override void enableGestureDrawing() { _drawingEnabled = true; }

    public override void disableGestureDrawing() { _drawingEnabled = false; }

    public override void setGesturesToRecognize(List<Gesture> gestures) { _recognizedGestures = gestures; /*Debug.Log("\t\t\t\t\t\t\tGestures set for recognition: " + gestures.Count);*/ }

    public override bool isEnabled() { return _drawingEnabled; }

    public override void clearGesturesToRecognize() { _recognizedGestures = new(); }

    // TODO: Note, currently the _drawingEnabled flag does nothing. There is no way to stop the gesture system from drawing. It may not be necessary,
    //   but if it ever becomes needed, this comment is here to make us aware that it not yet implemented
}
