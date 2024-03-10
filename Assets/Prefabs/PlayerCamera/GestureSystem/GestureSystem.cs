
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System;
using FMODUnity;

public class GestureSystem : AGestureSystem
{
    public static AGestureControllable ControllableInstance {get; private set;}  // SINGLETON
    
    private static readonly float TRAIL_COLLAPSE_FACTOR_FAST = 0.5f;  // How fast the trail vanishes while drawing
    private static readonly float TRAIL_COLLAPSE_FACTOR_SLOW = 0.05f;  // How fast the trail vanishes after releasing drawing
    private static readonly float DRAG_DIST_TO_ADD = 0.005f;  // When dragging, adds a mousepoint only if it is at least this distance away from the previous one as a percentage of the screen size
    private static readonly float MIN_GEST_DRAG_DIST = 0.17f;  // Distance to drag to be considered a valid gesture. Measured as percentage of screen w/h where max diagonal distance amounts to 1.41

    private StudioEventEmitter audioSys;

    private bool _drawingEnabled = false;
    private bool _onClickRunOnce = false;
    
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
        if (ControllableInstance != null && ControllableInstance != this) { Destroy(this); } 
        else { ControllableInstance = this; }

        audioSys = GetComponent<StudioEventEmitter>();
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
            _controls.Game.MousePos.ReadValue<Vector2>().x, _controls.Game.MousePos.ReadValue<Vector2>().y, 5));
        Vector3 linePos = cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(  // Get 3D position in front of camera
            _controls.Game.MousePos.ReadValue<Vector2>().x, _controls.Game.MousePos.ReadValue<Vector2>().y, 5.5f));
        trail.transform.position = particlePos;  // Move trail into position
        particle_system.transform.position = particlePos;  // Move particle system into postion
        MoveTrail();  // Collapse trail based on the collapse factor

        // Drawing gesture (mouse pressed)
        if (_controls.Game.Fire.IsPressed() && _drawingEnabled) {

            // fmod sound stuff
            if (!_onClickRunOnce) {
                audioSys.Play();
                audioSys.EventInstance.setParameterByName("ClickActive", 1);
                _onClickRunOnce = true;
            }

            Vector2 new_mouse_pos = _controls.Game.MousePos.ReadValue<Vector2>();
            Vector2 scaled_new_mouse_pos = new Vector2(new_mouse_pos.x / Screen.width, new_mouse_pos.y / Screen.height);
            Vector2 scaled_former_mouse_pos = mouseTrack.Count > 0 ? new Vector2(mouseTrack[mouseTrack.Count - 1].x / Screen.width, mouseTrack[mouseTrack.Count - 1].y / Screen.height): scaled_new_mouse_pos;
            float diff_mag = (scaled_former_mouse_pos - scaled_new_mouse_pos).magnitude;
            if (diff_mag > DRAG_DIST_TO_ADD || mouseTrack.Count == 0) {
                mouseTrack.Add(new_mouse_pos);  // Add user mouse point
                cum_dist += diff_mag;
                audioSys.EventInstance.setParameterByName("scribing_speed", diff_mag * 7000);  // Set play speed
            } 
            // Sends drawing event after a certain distance.
            if (cum_dist > MIN_GEST_DRAG_DIST && !began_drawing_event_sent) { _currentController.invokeBeganDrawingEvent(); began_drawing_event_sent = true; }

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

            if (_onClickRunOnce) {
                audioSys.EventInstance.setParameterByName("ClickActive", 0);
                _onClickRunOnce = false;
            }
                    
            // Only match if past length threshold AND drawing is enabled
            if (_drawingEnabled && cum_dist > MIN_GEST_DRAG_DIST) {

                bool matchFound = false;
                for (int g = 0; g < GesturesToRecognize.Count; g++) {  // Loop through registree's gestures
                    var gest = GesturesToRecognize[g];

                    // Dont match if its outside of a configured (only if configured) start point region
                    if (gest.LocationMaxRadius < 0 || (new Vector2(mouseTrack[0].x / Screen.width, mouseTrack[1].y / Screen.height) - gest.StartLocation).magnitude < gest.LocationMaxRadius) {
                        float acc = GestureUtils.compare_seq_to_gesture(mouseTrack, gest.Gest.ToList(), Const.NEXT_CHECKS, Const.MINIMIZATION_WEIGHTS, Const.FINAL_WEIGHTS, 0.01f);

                        if (acc < gest.SuccessAccuracy) {
                            _currentController.invokeGestureSuccessEvent(g);
                            matchFound = true;
                            break;
                        } else if (acc < gest.BackfireFailAccuracy) {
                            _currentController.invokeGestureBackfireEvent(g);
                            matchFound = true;
                            break;
                        }
                    }
                }
                if (!matchFound) { _currentController.invokeGestureFailEvent(); }  // If no match was found, invoke fail
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

    public override bool isEnabled() { return _drawingEnabled; }
}
