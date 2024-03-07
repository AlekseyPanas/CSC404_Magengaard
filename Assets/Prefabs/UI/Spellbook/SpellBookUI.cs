using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] private Button _SpellbookButton;
    [SerializeField] private RectTransform _SpellbookPanel;
    [SerializeField] private Image _FirstPage;
    [SerializeField] private Image _SecondPage;
    [SerializeField] private TextMeshProUGUI _LeftPageNum;
    [SerializeField] private TextMeshProUGUI _RightPageNum;
    
    private Vector2 _finalPanelRelativeSize = new Vector2(0.6f, 0.7f);  // Book panel image size relative to screen

    [Tooltip("The Gesture System")]
    [SerializeField] private AGestureControllable _gestSys;
    [Tooltip("Camera used by the gesture UI so that 3D UI objects can be drawn")]
    [SerializeField] private Camera _gestCamera;
    [Tooltip("Time in seconds that the mouse icons should move")]
    [SerializeField] private float _mouseIconLoopDuration;
    [SerializeField] private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;

    // Screen locations of book and arrows as a percentage of width and height (x,y) respectively
    private Vector2 _ArrowLeftRelScreenLoc = new(0.4f, 0.16f);
    private Vector2 _ArrowRightRelScreenLoc = new(0.6f, 0.16f);
    private Vector2 _ArrowCloseRelScreenLoc = new(0.86f, 0.73f);
    private Vector2 _BookModelRelScreenLoc = new(0.95f, 0.05f);

    // Arrow and book positions as pure screen pixel coordinates
    private Vector2 _arrowLeftPurePos;
    private Vector2 _arrowRightPurePos;
    private Vector2 _arrowClosePurePos;
    private Vector2 _bookModelPurePos;

    // Arrow and book positions in world coordinates
    private Vector3 _arrowLeftPureWorldPos;
    private Vector3 _arrowRightPureWorldPos;
    private Vector3 _arrowClosePureWorldPos;
    private Vector3 _bookModelPureWorldPos;

    // Arrow and book positions in world coordinates when they are off display
    private Vector3 _arrowLeftPureWorldFadedPos;
    private Vector3 _arrowRightPureWorldFadedPos;
    private Vector3 _arrowClosePureWorldFadedPos;
    private Vector3 _bookModelPureWorldFadedPos;

    // World coordinate distance to move the mouse icons along the arrows (shared across all mouse icons)
    private float _arrowMouseIconMoveDist;

    // World coordinate units to spawn the UI 3D objects in front of gesture camera
    private float _distFromCam = 7;

    // Percentage of screen height that the mouse icon should move along the arrow orthogonal direction to avoid overlapping with it
    private float _mouseIconOrthogonalDistFromArrowPercentHeight = 0.05f;

    // As a percentage of the mouse icon move duration, how much time to wait between icon movements
    private float _delayBetweenMouseIconsMovementsPercentDuration = 1.5f;

    private bool _isSpellbookInInventory = false;  // Set to true once the spellbook is first picked up
    private bool _isPickupSysBusy = false;  // Is the pickup system currently inspecting something (could be this very book)
    private float _openPercentage = 0;  // 1 means fully open, 0 means fully closed
    private float _openCloseDuration = 1f;  // Time in second to open and close the UI
    private float _iconAddedPercentage = 0;  // 1 means the icon fully faded in, 0 means not at all
    private float _iconMoveInDuration = 1f;  // Time for the book icon to move into view

    private int _PickupablesListBookIndex = (int)PickupablesNetworkPrefabListIndexes.BOOK;  // prefab that the pickup system should spawn out of JJ's pocket
    
    private Quaternion _baseBookRot;  // Records initial book rotation to allow modification

    private List<Gesture> _uiGestures;  // The gestures used by this UI to turn pages and close UI
    private GestureSystemControllerRegistrant _gestRegistrant;  // When UI is open, keeps track of registree data with the gesture system
    private ControllerRegistrant _pickupSysRegistrant;

    private DesktopControls _controls;

    private int _page = 0;
    private List<Sprite> _pageImages = new List<Sprite>();

    void Start() {
        // Compute the pure screen position of the arrows. Then the world position. Then move and rotate each arrow to the correct location
        _arrowLeftPurePos = ScreenPosFromRelative(_ArrowLeftRelScreenLoc);
        _arrowLeftPureWorldPos = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowLeftPurePos.x, _arrowLeftPurePos.y, _distFromCam));
        _arrowLeftPureWorldFadedPos =  _gestCamera.ScreenToWorldPoint(new Vector3(0 - Screen.width * 0.1f, _arrowLeftPurePos.y, _distFromCam));
        _ArrowLeft.position = _arrowClosePureWorldFadedPos;
        _ArrowLeft.rotation = _gestCamera.transform.rotation;
        _ArrowLeft.Rotate(new Vector3(0, 90, 90));

        _arrowRightPurePos = ScreenPosFromRelative(_ArrowRightRelScreenLoc);
        _arrowRightPureWorldPos = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowRightPurePos.x, _arrowRightPurePos.y, _distFromCam));
        _arrowRightPureWorldFadedPos =  _gestCamera.ScreenToWorldPoint(new Vector3(Screen.width + Screen.width * 0.1f, _arrowRightPurePos.y, _distFromCam));
        _ArrowRight.position = _arrowRightPureWorldFadedPos;
        _ArrowRight.rotation = _gestCamera.transform.rotation;
        _ArrowRight.Rotate(new Vector3(180, 90, 90));

        _arrowClosePurePos = ScreenPosFromRelative(_ArrowCloseRelScreenLoc);
        _arrowClosePureWorldPos = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowClosePurePos.x, _arrowClosePurePos.y, _distFromCam));
        _arrowClosePureWorldFadedPos = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowClosePurePos.x, Screen.height + Screen.height * 0.1f, _distFromCam));
        _ArrowClose.position = _arrowClosePureWorldFadedPos;
        _ArrowClose.rotation = _gestCamera.transform.rotation;
        _ArrowClose.Rotate(new Vector3(45, 90, 90));

        _bookModelPurePos = ScreenPosFromRelative(_BookModelRelScreenLoc);
        _bookModelPureWorldPos = _gestCamera.ScreenToWorldPoint(new Vector3(_bookModelPurePos.x, _bookModelPurePos.y, _distFromCam));
        _bookModelPureWorldFadedPos = _gestCamera.ScreenToWorldPoint(new Vector3(Screen.width + Screen.width * 0.1f, _bookModelPurePos.y, _distFromCam));
        _BookIconModel.position = _bookModelPureWorldFadedPos;
        _BookIconModel.rotation = _gestCamera.transform.rotation;
        _BookIconModel.Rotate(new Vector3(0, 190, 0));
        _baseBookRot = _BookIconModel.rotation;

        // Compute the world move distance of the mouse icons
        _arrowMouseIconMoveDist = ScreenDistToWorldDist(Screen.height * 0.3f, _distFromCam);  

        // Configures UI gestures
        _uiGestures = new List<Gesture> { 
            new Gesture(new List<GestComp>{ new GestComp(180, 1) }, 0.2f, -1f, _ArrowLeftRelScreenLoc, 0.1f),
            new Gesture(new List<GestComp>{ new GestComp(0, 1) }, 0.2f, -1f, _ArrowRightRelScreenLoc, 0.1f),
            new Gesture(new List<GestComp>{ new GestComp(135, 1) }, 0.2f, -1f, _ArrowCloseRelScreenLoc, 0.1f) 
        };

        // Enable input system
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        // Register to open spellbook
        _SpellbookButton.GetComponent<Button>().onClick.AddListener(OnSpellbookButtonClick);

        // Sets busy based on pickup system event
        PickupSystem.OnTogglePickupSequence += (bool isBusy) => {
            _isPickupSysBusy = isBusy;
        };

        // Make book invisible
        _BookPanel.sizeDelta = new Vector2(0, 0);

        // Add pages as they come
        FallenPageUI.PagePickedUpEvent += (Sprite sprite) => {
            _pageImages.Add(sprite);
        };

        _LeftPageNum.gameObject.SetActive(false);
        _RightPageNum.gameObject.SetActive(false);
    }

    /* Registers with gesture system **/
    void SetGesturesToRecognize() {
        _gestSys.GetSystem(_gestRegistrant)?.SetGesturesToRecognize(_uiGestures);
        _gestRegistrant.GestureSuccessEvent += OnGestureSuccess;
    }

    /** On button click, initialize unpocketing */
    void OnSpellbookButtonClick() {
        if (_isSpellbookInInventory && !_isPickupSysBusy) {
            _pickupSysRegistrant = _pickupSys.RegisterController(1);
            if (_pickupSysRegistrant == null) { return; }

            _pickupSys.GetSystem(_pickupSysRegistrant)?.StartUnpocketing(_PickupablesListBookIndex, gameObject);
            _pickupSysRegistrant.OnInterrupt += OnBookClose;
        }
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

    void UpdatePageSprites() {
        int i1 = _page * 2;
        int i2 = _page * 2 + 1;
    
        _LeftPageNum.SetText(i1.ToString());
        _RightPageNum.SetText(i2.ToString());

        if (i1 < _pageImages.Count) {
            _FirstPage.sprite = _pageImages[i1];
            _FirstPage.color = new Color(1, 1, 1, 1);
        } else { _FirstPage.color = new Color(1, 1, 1, 0); }

        if (i2 < _pageImages.Count) {
            _FirstPage.sprite = _pageImages[i2];
            _FirstPage.color = new Color(1, 1, 1, 1);
        } else { _SecondPage.color = new Color(1, 1, 1, 0); }
    }
    
    void OnPageTurnRight() {
        _page++;
        UpdatePageSprites();
    }

    void OnPageTurnLeft() {
        if (_page > 0) {
            _page--;
            UpdatePageSprites();
        }
    }

    void OnBookClose() {
        StartCoroutine(CloseUI());
        _pickupSys.DeRegisterController(_pickupSysRegistrant);
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

        // Moves book around to mouse position
        var mousePos = _controls.Game.MousePos.ReadValue<Vector2>();
        _BookIconModel.rotation = _baseBookRot;
        _BookIconModel.Rotate(new Vector3(-(mousePos.y / Screen.height) * 10, (mousePos.x / Screen.width) * 10, 0));

        // Lerps book and arrows based on transition percentages
        _ArrowLeft.position = Vector3.Lerp(_arrowLeftPureWorldFadedPos, _arrowLeftPureWorldPos, easingFunc(_openPercentage));
        _ArrowRight.position = Vector3.Lerp(_arrowRightPureWorldFadedPos, _arrowRightPureWorldPos, easingFunc(_openPercentage));
        _ArrowClose.position = Vector3.Lerp(_arrowClosePureWorldFadedPos, _arrowClosePureWorldPos, easingFunc(_openPercentage));
        _BookIconModel.position = Vector3.Lerp(_bookModelPureWorldFadedPos, _bookModelPureWorldPos, easingFunc(_iconAddedPercentage));
        _BookPanel.sizeDelta = Vector2.Lerp(new Vector2(0, 0), 
                        new Vector2(_finalPanelRelativeSize.x * Screen.width, _finalPanelRelativeSize.y * Screen.height), easingFunc(_openPercentage));
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

    /** given a value 0 to 1, returns an eased version between 0 and 1 */
    float easingFunc(float x) { return (float)(0.5 * Math.Sin(Math.PI * x - (Math.PI / 2)) + 0.5); }

    IEnumerator MoveIconIn() {
        float startTime = Time.time;
        while (_iconAddedPercentage < 1) {
            _iconAddedPercentage = Math.Min((Time.time - startTime) / _iconMoveInDuration, 1f);
            yield return null;
        }
    }

    IEnumerator CloseUI() {
        _LeftPageNum.gameObject.SetActive(false);
        _RightPageNum.gameObject.SetActive(false);

        float startTime = Time.time;
        while (_openPercentage > 0) {
            _openPercentage = 1 - Math.Min((Time.time - startTime) / _openCloseDuration, 1f);
            yield return null;
        }
    }

    IEnumerator OpenUI() {
        UpdatePageSprites();

        float startTime = Time.time;
        while (_openPercentage < 1) {
            _openPercentage = Math.Min((Time.time - startTime) / _openCloseDuration, 1f);
            yield return null;
        }
        SetGesturesToRecognize();
        _LeftPageNum.gameObject.SetActive(true);
        _RightPageNum.gameObject.SetActive(true);
    }

    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        // Add spellbook to inventory if first time
        if (!_isSpellbookInInventory) {
            _isSpellbookInInventory = true;
            StartCoroutine(MoveIconIn());
            _pickupSys.DeRegisterController(pickupRegistrant);  // End immediately on the first time
        } 
        
        // Open spellbook UI
        else {
            StartCoroutine(OpenUI());
            _gestRegistrant = gestureRegistrant;
        }
    }
}
