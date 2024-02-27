using System;
using System.Collections.Generic;
using UnityEngine;


/** UI for the spellbook which can be re-opened  */
public class SpellBookUI : MonoBehaviour, IInspectable
{
    [SerializeField] private RectTransform _BookPanel;
    [SerializeField] private Transform _ArrowLeft;
    [SerializeField] private Transform _ArrowRight;
    [SerializeField] private Transform _ArrowClose;
    [SerializeField] private Transform _ArrowLeftMouse;
    [SerializeField] private Transform _ArrowRightMouse;
    [SerializeField] private Transform _ArrowCloseMouse;
    [SerializeField] private Transform _BookIconModel;


    [Tooltip("The Gesture System")]
    [SerializeField] private GestureSystem _gestSys;
    [Tooltip("Camera used by the gesture UI so that 3D UI objects can be drawn")]
    [SerializeField] private Camera _gestCamera;
    [Tooltip("Time in seconds that the mouse icons should move")]
    [SerializeField] private float _mouseIconLoopDuration;

    // Screen locations of book and arrows as a percentage of width and height (x,y) respectively
    private Vector2 _ArrowLeftRelScreenLoc = new(0.4f, 0.16f);
    private Vector2 _ArrowRightRelScreenLoc = new(0.6f, 0.16f);
    private Vector2 _ArrowCloseRelScreenLoc = new(0.8f, 0.8f);
    private Vector2 _BookModelRelScreenLoc = new(0.95f, 0.05f);

    // Arrow and book positions as pure screen pixel coordinates
    private Vector2 _arrowLeftPurePos;
    private Vector2 _arrowRightPurePos;
    private Vector2 _arrowClosePurePos;
    private Vector2 _bookModelPurePos;

    // World coordinate distance to move the mouse icons along the arrows (shared across all mouse icons)
    private float _arrowMouseIconMoveDist;

    // World coordinate units to spawn the UI 3D objects in front of gesture camera
    private float _distFromCam = 7;

    // Percentage of screen height that the mouse icon should move along the arrow orthogonal direction to avoid overlapping with it
    private float _mouseIconOrthogonalDistFromArrowPercentHeight = 0.05f;

    // As a percentage of the mouse icon move duration, how much time to wait between icon movements
    private float _delayBetweenMouseIconsMovementsPercentDuration = 1.5f;

    private bool isSpellbookInInventory = false;  // Set to true once the spellbook is first picked up
    private int _PickupablesListBookIndex = (int)PickupablesNetworkPrefabListIndexes.BOOK;  // prefab that the pickup system should spawn out of JJ's pocket
    private GestureSysRegistree _registree;
    private List<Gesture> _uiGestures;

    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };  // Call this when the item is unpocketed

    void Start() {
        // Compute the pure screen position of the arrows. Then the world position. Then move and rotate each arrow to the correct location
        _arrowLeftPurePos = ScreenPosFromRelative(_ArrowLeftRelScreenLoc);
        _ArrowLeft.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowLeftPurePos.x, _arrowLeftPurePos.y, _distFromCam));
        _ArrowLeft.rotation = _gestCamera.transform.rotation;
        _ArrowLeft.Rotate(new Vector3(0, 90, 90));

        _arrowRightPurePos = ScreenPosFromRelative(_ArrowRightRelScreenLoc);
        _ArrowRight.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowRightPurePos.x, _arrowRightPurePos.y, _distFromCam));
        _ArrowRight.rotation = _gestCamera.transform.rotation;
        _ArrowRight.Rotate(new Vector3(180, 90, 90));

        _arrowClosePurePos = ScreenPosFromRelative(_ArrowCloseRelScreenLoc);
        _ArrowClose.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowClosePurePos.x, _arrowClosePurePos.y, _distFromCam));
        _ArrowClose.rotation = _gestCamera.transform.rotation;
        _ArrowClose.Rotate(new Vector3(45, 90, 90));

        _bookModelPurePos = ScreenPosFromRelative(_BookModelRelScreenLoc);
        _BookIconModel.position = _gestCamera.ScreenToWorldPoint(new Vector3(_bookModelPurePos.x, _bookModelPurePos.y, _distFromCam));
        _BookIconModel.rotation = _gestCamera.transform.rotation;
        _BookIconModel.Rotate(new Vector3(0, 190, 0));

        // Compute the world move distance of the mouse icons
        _arrowMouseIconMoveDist = ScreenDistToWorldDist(Screen.height * 0.3f, _distFromCam);  

        // Configures UI gestures
        _uiGestures = new List<Gesture> { 
            new Gesture(new List<GestComp>{ new GestComp(180, 1) }, 0.2f, -1f, _ArrowLeftRelScreenLoc, 0.1f),
            new Gesture(new List<GestComp>{ new GestComp(0, 1) }, 0.2f, -1f, _ArrowRightRelScreenLoc, 0.1f),
            new Gesture(new List<GestComp>{ new GestComp(135, 1) }, 0.2f, -1f, _ArrowCloseRelScreenLoc, 0.1f) 
        };

        RegisterWithGestSys();
    }

    /* Registers with gesture system **/
    void RegisterWithGestSys() {
        _registree = _gestSys.RegisterNewListener((int)GestureSystemPriorityLayers.UI);
        _registree.SetGesturesToRecognize(_uiGestures);
        _registree.GestureSuccessEvent += OnGestureSuccess;
    }

    /* Deregisters from gesture system to give control back to spells **/
    void DeRegisterGestSys() {
        _registree.GestureSuccessEvent -= OnGestureSuccess;
        _gestSys.DeRegisterListener(_registree.registreeId);
    }

    void OnGestureSuccess(int index) {
        switch (index) {
            case 0:
                OnPageTurnLeft();
                break;
            case 1:
                OnPageTurnRight();
                break;
            case 2:
                OnBookClose();
                break;
            default:
                break;
        }
    }

    void OnPageTurnRight() {
        Debug.Log("Page turned right");
    }

    void OnPageTurnLeft() {
        Debug.Log("Page turned Left");
    }

    void OnBookClose() {
        Debug.Log("Book Closed");
    }

    void Update () {
        // Compute the looping percentage of the mouse icon motion. Temp variables used to scale the computation to include a gap in the animation
        float _temp = _mouseIconLoopDuration * (1 + _delayBetweenMouseIconsMovementsPercentDuration);
        float _temp2 = 1 / (1 + _delayBetweenMouseIconsMovementsPercentDuration); 
        float percentComplete = (float)(Time.time % _temp / _temp);

        if (percentComplete <= _temp2) {
            percentComplete *= 1 + _delayBetweenMouseIconsMovementsPercentDuration;

            List<Transform> _arrows = new List<Transform> {_ArrowLeft, _ArrowRight, _ArrowClose};
            List<Transform> _mouseIcons = new List<Transform> {_ArrowLeftMouse, _ArrowRightMouse, _ArrowCloseMouse};

            for (int i = 0; i < 3; i++) {
                // Move the mouse icon along the forward vector of the arrow
                _mouseIcons[i].position = _arrows[i].position - (_arrows[i].forward.normalized * (percentComplete * _arrowMouseIconMoveDist));
                // Move the mouse icon along the right vector so that it doesnt overlap with the arrow
                _mouseIcons[i].position += (i == 0 ? -1 : 1) * _arrows[i].right.normalized * ScreenDistToWorldDist(Screen.height * _mouseIconOrthogonalDistFromArrowPercentHeight, _distFromCam);
                // Fade the color using the parabolic curve
                var col = _mouseIcons[i].GetComponent<SpriteRenderer>().color;
                _mouseIcons[i].GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0.6f * fadeInThenOutZeroToOne(percentComplete));
            }
        }


        
        if (isOpen && Input.GetKeyDown(KeyCode.X)) {  // TODO REPLACE WITH GESTURE SYSTEM
            isOpen = false;
            OnInspectEnd();
        } else if (!isOpen && Input.GetKeyDown(KeyCode.B)) {
            OnUnpocketInspectableEvent(_PickupablesListBookIndex, gameObject);
        }
    }

    /** Convert a 2D screen position from being a percentage of width, height to being a screen pixel coordinate */
    Vector2 ScreenPosFromRelative(Vector2 relativePos) {
        return new Vector2(relativePos.x * Screen.width, relativePos.y * Screen.height);
    }

    /** Given a forward distance from the camera and a screen pixel coordinate distance, convert to world coordinate distance */
    float ScreenDistToWorldDist(float screenDist, float distFromCam) {
        return (_gestCamera.ScreenToWorldPoint(new Vector3(0, 0, _distFromCam)) - _gestCamera.ScreenToWorldPoint(new Vector3(screenDist, 0, distFromCam))).magnitude;
    }

    /** An inverse parabola such that it peaks at 1 when x = 0.5, and goes to 0 at x = 1 and x = 0. Used for smooth fading in and out */
    float fadeInThenOutZeroToOne(float x) { return (float)(-4f * Math.Pow(x - 0.5f, 2f) + 1f); }

    public void OnInspectStart(Action OnInspectEnd) {
        // isOpen = true;
        // this.OnInspectEnd = OnInspectEnd;
    }

        
}
