using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;


public class NewSpellbookUI : MonoBehaviour, IInspectable {

    enum BookStates {
        CLOSED = 0,
        OPENING = 1,
        OPEN = 2,
        TURNING_RIGHT = 3,
        TURNING_LEFT = 4,
        CLOSING = 5,
        FLIPPING = 6,
        AUTO_CLOSING = 7
    }

    [Tooltip("Camera used by the spellbook UI")] [SerializeField] private Camera _cam;
    [SerializeField] private GameObject _bookPagePrefab;

    [SerializeField] private Texture2D _pageBaseTexture;  // Full texture for a blank page on both sides
    [SerializeField] private Texture2D _pageBaseNormalMap;  // Full normap map for a blank page on both sides
    [SerializeField] private Texture2D _pageBaseHalfPageTexture;  // texture for blank one side of a page
    [SerializeField] private Texture2D _pageBaseHalfPageNormalMap;  // normal map for blank one side of a page 
    [SerializeField] private Texture2D _pageHalfPageNotifTexture;  // Overlay texture of "new" notification

    [SerializeField] public EventReference _openSoundPath;
    [SerializeField] public EventReference _closeSoundPath;    

    [SerializeField] private Transform _leftCoverBone;
    [SerializeField] private Transform _rightCoverBone;

    [SerializeField] private Transform _pageSpawnRelativePos;  // Position relative to book origin where pages are instantiated

    // As the page flips, it goes from the left cover, through the middle, to the right cover. These anchors guide the animation
    [SerializeField] private Transform _leftCoverCurveAnchor;
    [SerializeField] private Transform _rightCoverCurveAnchor;
    [SerializeField] private Transform _middleCurveAnchor;

    // x-axis rotation values for the cover control bones when in fully closed or fully open configuration
    [SerializeField] private float _leftCoverClosedRotation;
    [SerializeField] private float _leftCoverOpenRotation;
    [SerializeField] private float _rightCoverClosedRotation;
    [SerializeField] private float _rightCoverOpenRotation;

    // When book is an icon, cover faces you. When book is open, pages face you. These are the two rotation endpoints for the entire book object
    [SerializeField] private Vector3 _bookClosedRotation;
    [SerializeField] private Vector3 _bookOpenRotation;

    // Used to position the book
    [SerializeField] private Transform _bookSpineCenterTransform;  // Object at the center of the book's spine (i.e xz local positions are 0, only y)
    [SerializeField] private float _bookOpenDistFromCam;
    [SerializeField] private float _bookClosedDistFromCam;
    [SerializeField] private Vector2 _bookClosedOffsetBottomLeftScreenPercentHeight;  // Books location offset from bottom left screen corner as percentage of screen height

    // The left-right axis keypoints
    [SerializeField] private float _xHidden;
    [SerializeField] private float _xBackup;
    [SerializeField] private float _xShowing;
    [SerializeField] private float _xClosed;

    // Interactive stuff
    [SerializeField] private Transform _leftNode;
    [SerializeField] private Transform _rightNode;
    [SerializeField] private Transform _coverNode;
    private ParticleSystem _leftNodeParticleSys;
    private ParticleSystem _rightNodeParticleSys;
    private ParticleSystem _coverNodeParticleSys;
    [SerializeField] private float _interactDistanceThreshold;  // Percent of screen height to register node grab
    [SerializeField] private Transform _iconBookCenter;  // Center location from where click detection radius stems

    // How far to drag mouse to fully turn page or close book (as percentage of screen width)
    [SerializeField] private float _dragScreenWidthPercentToTurnPage;
    [SerializeField] private float _percentToCommitFlip;  // If page is turned at least this much, itll commit to the flip

    // Mouse icons
    [SerializeField] private Texture2D _cursorOpenHand;
    [SerializeField] private Texture2D _cursorFist;

    // Animation times
    [SerializeField] private float _OpenCloseTime;
    [SerializeField] private float _flipPageTime;

    private Vector2 _coverLeftDefaultEulerAnglesYZ;
    private Vector2 _coverRightDefaultEulerAnglesYZ;

    private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;
    private ControllerRegistrant _pickupSysRegistrant;
    private DesktopControls _controls;
    private StudioEventEmitter _emitterOpen;
    private StudioEventEmitter _emitterClose;

    private bool _isSpellbookInInventory = false;  // Set to true once the spellbook is first picked up
    private BookStates _state = BookStates.CLOSED;

    private List<BookPage> _pages = new();

    private List<Texture2D> _contentNotif = new();  // Texture for each half page of content with notif overlay
    private List<Texture2D> _contentNormals = new();  // Normal map for each half page of content 
    private List<Texture2D> _content = new();  // Texture for each half page with no notif
    private List<int> _unseenContent = new();  // List of indexes of unseen pages (one index per half of content texture, i.e one idx per one side of a page)

    private List<Tuple<Texture2D, Texture2D, Texture2D>> _newContentQueue = new();
    private int _curRightPageIdx;  // When book is open, tracks the index of the page on the left
    private bool _wasMouseDownLastFrame = false;

    private float _mouseXWhenStartedDragging;  // Records mouse position when dragging started

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };

        ASpellbookContributor.OnContributeContent += (Texture2D t, Texture2D t1, Texture2D t2) => { _newContentQueue.Add(new(t, t1, t2)); };
        ASpellbookContributor.GetFullPageDims += () => { return _bookPagePrefab.GetComponent<BookPage>().FullTextureDims; };
        ASpellbookContributor.GetHalfPageDims += () => { return _bookPagePrefab.GetComponent<BookPage>().HalfPageDims; };
        ASpellbookContributor.GetHalfPageNotifTexture += () => { return _pageHalfPageNotifTexture; };
        ASpellbookContributor.GetBaseHalfPageTexture += () => { return _pageBaseHalfPageTexture; };
        ASpellbookContributor.GetBaseHalfPageNormalMap += () => { return _pageBaseHalfPageNormalMap; };

        _emitterOpen = gameObject.AddComponent<StudioEventEmitter>();
        _emitterOpen.EventReference = _openSoundPath;
        _emitterClose = gameObject.AddComponent<StudioEventEmitter>();
        _emitterClose.EventReference = _closeSoundPath;

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        _leftNodeParticleSys = _leftNode.gameObject.GetComponent<ParticleSystem>();
        _rightNodeParticleSys = _rightNode.gameObject.GetComponent<ParticleSystem>();
        _coverNodeParticleSys = _coverNode.gameObject.GetComponent<ParticleSystem>();
        _StopAllNodeParticleSystems();

        // Enable input system
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        _coverLeftDefaultEulerAnglesYZ = new Vector2(_leftCoverBone.localEulerAngles.y, _leftCoverBone.localEulerAngles.z);
        _coverRightDefaultEulerAnglesYZ = new Vector2(_rightCoverBone.localEulerAngles.y, _rightCoverBone.localEulerAngles.z);

        _SetBookOpenPercent(0);
        _SetBookOpenPositionPercent(0);

        _ProcessNewContentQueue();
    }

    /** On button click, initialize unpocketing */
    void OnSpellbookButtonClick() {
        // if (_isSpellbookInInventory && !_isPickupSysBusy) {
        //     _pickupSysRegistrant = _pickupSys.RegisterController(1);
        //     if (_pickupSysRegistrant == null) { return; }

        //     _pickupSys.GetSystem(_pickupSysRegistrant)?.StartUnpocketing(_PickupablesListBookIndex, gameObject);
        //     _pickupSysRegistrant.OnInterrupt += OnBookClose;
        // }
    }

    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        // // Add spellbook to inventory if first time
        // if (!_isSpellbookInInventory) {
        //     _isSpellbookInInventory = true;
        //     StartCoroutine(MoveIconIn());
        //     _pickupSys.DeRegisterController(pickupRegistrant);  // End immediately on the first time
        // } 
        
        // // Open spellbook UI
        // else {
        //     StartCoroutine(OpenUI());
        //     _gestRegistrant = gestureRegistrant;

        //     _emitterOpen.Play();
        // }
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// █▄▀ █▀▀ █▄█   █░█ ▀█▀ █ █░░ █ ▀█▀ █ █▀▀ █▀
// █░█ ██▄ ░█░   █▄█ ░█░ █ █▄▄ █ ░█░ █ ██▄ ▄█
/////////////////////////////////////////////////////////////////////////////////////////////////////////

private void _ProcessNewContentQueue() {
    foreach (var tup in _newContentQueue) {
        _OnContentContribute(tup.Item1, tup.Item2, tup.Item3);
    } _newContentQueue = new();
}

private void _StopAllNodeParticleSystems() {
    _ToggleParticle(false, _leftNodeParticleSys);
    _ToggleParticle(false, _rightNodeParticleSys);
    _ToggleParticle(false, _coverNodeParticleSys);
}

private void _ToggleParticle(bool toggle, ParticleSystem particleSys) {
    if (toggle) {
        particleSys.Play();  
    }
    else {
        particleSys.Stop(); 
        particleSys.Clear();
    }
}

private bool _ExistsPage(int index) {
    return index >= 0 && index < _pages.Count;
}

/** 
* Add one new page side of content into the spellbook
* Assumes book is closed
*/
private void _OnContentContribute(Texture2D texture, Texture2D textureWithNotif, Texture2D normalMap) {
    _contentNormals.Add(normalMap);
    _content.Add(texture);
    _contentNotif.Add(textureWithNotif);
    _unseenContent.Add(_contentNotif.Count - 1);

    if ((_content.Count-1) % 2 == 0) {
        _SpawnNewPage();
        
        // _SetBookOpenPercent(0);
        // _SetBookOpenPositionPercent(0);
    } 
    
    else {
        _pages[_pages.Count - 1].SetRightTexture(_contentNotif[_content.Count - 1]);
        _pages[_pages.Count - 1].SetRightNormal(_contentNormals[_content.Count - 1]);
    }   
}

/** 
* Spawn a new spell page into the book in closed position using the latest content
*/
private void _SpawnNewPage() {
    // Position and parent the page
    var obj = Instantiate(_bookPagePrefab, transform);
    obj.transform.localPosition = _pageSpawnRelativePos.transform.localPosition;
    var bookPage = obj.GetComponent<BookPage>();
    _SetPageOpenPercent(bookPage, 0, _xClosed, _xClosed);
    
    //Vector2 effectorXZ = _GetPageFlipPosition(x, 1);
    //bookPage.PageEffector.transform.position = new Vector3(effectorXZ.x, bookPage.PageEffector.transform.position.y, effectorXZ.y);

    // Set texture
    bookPage.SetLeftTexture(_contentNotif[_content.Count-1]);
    bookPage.SetRightTexture(_pageBaseTexture);

    bookPage.SetLeftNormal(_contentNormals[_content.Count-1]);
    bookPage.SetRightNormal(_pageBaseNormalMap);
    
    _pages.Add(bookPage);
}

/** 
* Curve for smoothly turning the page between the anchors
*/
private double _PageFlipCurve(double x) {
    return ( (-0.55 * Math.Tanh(3 * (x - 0.5)) + 0.5) + (Mathf.Sqrt(1-Mathf.Pow((float)x, 2))) ) / 2;
}

/** 
* x is an input along the axis between the left and right cover anchors.
* x=0 is the point on that axis where the middle anchor projects onto it
* x=-1 is the left anchor
* x=1 is the right anchor
* This returns a world space vector2 xz plane position of where an effector should go
* Scale allows specifying a left-right anchor axis scale modifier
*/
private Vector3 _GetPageFlipPosition(double x, float xscale = 1, float yscale = 1) {
    // Project anchors to xz
    Vector3 left = _leftCoverCurveAnchor.position; //new Vector2(_leftCoverCurveAnchor.position.x, _leftCoverCurveAnchor.position.z);
    Vector3 right = _rightCoverCurveAnchor.position; //new Vector2(_rightCoverCurveAnchor.position.x, _rightCoverCurveAnchor.position.z);
    Vector3 middle = _middleCurveAnchor.position; //new Vector2(_middleCurveAnchor.position.x, _middleCurveAnchor.position.z);

    // Get positive direction along left-to-right anchor axis
    Vector3 xPosDir = (right - left).normalized;

    // Find the projection of middle anchor onto left-to-right anchor axis. Use that to get y direction
    float projection_mag = Vector3.Dot(middle - left, xPosDir);
    Vector3 proj = left + (projection_mag * xPosDir);
    Vector3 yDir = (middle - proj) * yscale;

    Vector3 xPosFull = right - proj;
    Vector3 xNegFull = left - proj;

    // Use page curve to get amount along orthogonal "y" direction, combine with x direction
    if (x > 0) {
        return proj + (Mathf.Abs((float)x) * xscale * xPosFull) + ((float)_PageFlipCurve(Mathf.Abs((float)x)) * yDir);
    } else {
        return proj + (Mathf.Abs((float)x) * xscale * xNegFull) + ((float)_PageFlipCurve(Mathf.Abs((float)x)) * yDir);
    }
}

/** Deals with moving the book in 3D space from small icon view to close up open view. Includes dealing with the slight orientation*/
private void _SetBookOpenPositionPercent(float percent) {
    Vector3 iconPosScreenSpace = new Vector3(Screen.width - _bookClosedOffsetBottomLeftScreenPercentHeight.x * Screen.height, 
                                    _bookClosedOffsetBottomLeftScreenPercentHeight.y * Screen.height, _bookClosedDistFromCam);
    Vector3 iconPosWorldSpace = _cam.ScreenToWorldPoint(iconPosScreenSpace);
    Vector3 openPosWorldSpace = _cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, _bookOpenDistFromCam));

    Vector3 diff = _bookSpineCenterTransform.position - transform.position;
    transform.position = (iconPosWorldSpace - diff) + 
                        ((openPosWorldSpace - diff) - 
                        (iconPosWorldSpace - diff)) * percent;
    transform.localEulerAngles = new Vector3(
        Const.interp(_bookClosedRotation.x, _bookOpenRotation.x, percent),
        transform.localEulerAngles.y, 
        Const.interp(_bookClosedRotation.z, _bookOpenRotation.z, percent));
}

/** Deals with rotating the book from icon orientation to close up screen-facing orientation, with 0 being full icon and 1 being full open */
private void _SetBookOpenRotationPercent(float percent) {
    transform.localEulerAngles = new Vector3(
        transform.localEulerAngles.x,
        Const.interp(_bookClosedRotation.y, _bookOpenRotation.y, percent), 
        transform.localEulerAngles.z);
}

/** Moves effectors and rotates cover bones so that the book is open or closed based on percent. This does not modify
individual content pages -- just the book base */
private void _SetCoverOpenPercent(float percent) {
    _leftCoverBone.localEulerAngles = new Vector3(Const.interp(_leftCoverClosedRotation, _leftCoverOpenRotation, percent), _coverLeftDefaultEulerAnglesYZ.x, _coverLeftDefaultEulerAnglesYZ.y);
    _rightCoverBone.localEulerAngles = new Vector3(Const.interp(_rightCoverClosedRotation, _rightCoverOpenRotation, percent), _coverRightDefaultEulerAnglesYZ.x, _coverRightDefaultEulerAnglesYZ.y);

    //_leftPagesEffector.position = Const.WithY(_GetPageFlipPosition(Const.interp(0, -1, percent), 1, 1.05f), _leftPagesEffector.position.y);
    //_rightPagesEffector.position = Const.WithY(_GetPageFlipPosition(Const.interp(0, 1, percent), 1, 1.05f), _rightPagesEffector.position.y);
}

/** Interpolates and sets effector of single page along percent between from and to */
private void _SetPageOpenPercent(BookPage page, float percent, float xFrom, float xTo) {

    page.PageEffector.position = _GetPageFlipPosition(Const.interp(xFrom, xTo, percent)) + (transform.up.normalized * _pageSpawnRelativePos.localPosition.y * 4f);  // 4 is the book's scale

    //page.PageEffector.position = Const.WithY(_GetPageFlipPosition(Const.interp(xFrom, xTo, percent)), page.PageEffector.position.y);
}

/** Using the cur right page index to where the book is open, and given isRight (false means left page), compute intepolation
of all pages that need to be moved throughout the flip */
private void _SetPageFlipPercent(bool isRight, float percent) {
    if (_pages.Count == 0) { return; }
    if (isRight) {
        if (_ExistsPage(_curRightPageIdx-2)) _SetPageOpenPercent(_pages[_curRightPageIdx-2], percent, -_xBackup, -_xHidden);
        if (_ExistsPage(_curRightPageIdx-1)) _SetPageOpenPercent(_pages[_curRightPageIdx-1], percent, -_xShowing, -_xBackup);
        if (_ExistsPage(_curRightPageIdx)) _SetPageOpenPercent(_pages[_curRightPageIdx], percent, _xShowing, -_xShowing);
        if (_ExistsPage(_curRightPageIdx+1)) _SetPageOpenPercent(_pages[_curRightPageIdx+1], percent, _xBackup, _xShowing);
        if (_ExistsPage(_curRightPageIdx+2)) _SetPageOpenPercent(_pages[_curRightPageIdx+2], percent, _xHidden, _xBackup);
    } else {
        if (_ExistsPage(_curRightPageIdx-3)) _SetPageOpenPercent(_pages[_curRightPageIdx-3], percent, -_xHidden, -_xBackup);
        if (_ExistsPage(_curRightPageIdx-2)) _SetPageOpenPercent(_pages[_curRightPageIdx-2], percent, -_xBackup, -_xShowing);
        if (_ExistsPage(_curRightPageIdx-1)) _SetPageOpenPercent(_pages[_curRightPageIdx-1], percent, -_xShowing, _xShowing);
        if (_ExistsPage(_curRightPageIdx)) _SetPageOpenPercent(_pages[_curRightPageIdx], percent, _xShowing, _xBackup);
        if (_ExistsPage(_curRightPageIdx+1)) _SetPageOpenPercent(_pages[_curRightPageIdx+1], percent, _xBackup, _xHidden);
    }
}

/** Combines cover, page stack, and individual page transforms to open or close the book */
private void _SetBookOpenPercent(float percent) {
    _SetCoverOpenPercent(percent);
    _SetBookOpenRotationPercent(percent);

    for (int p = 0; p < _pages.Count; p++) {
        if (p == _curRightPageIdx - 2) {
            _SetPageOpenPercent(_pages[p], percent, _xClosed, -_xBackup);
        } else if (p == _curRightPageIdx - 1) { 
            _SetPageOpenPercent(_pages[p], percent, _xClosed, -_xShowing);
        } else if (p == _curRightPageIdx) {
            _SetPageOpenPercent(_pages[p], percent, _xClosed, _xShowing);
        } else if (p == _curRightPageIdx + 1) {
            _SetPageOpenPercent(_pages[p], percent, _xClosed, _xBackup);
        } else if (p < _curRightPageIdx) {
            _SetPageOpenPercent(_pages[p], percent, _xClosed, -_xHidden);
        } else {
            _SetPageOpenPercent(_pages[p], percent, _xClosed, _xHidden);
        }
    }
}

/** 
* Given the index of one side of a page (e.g a single two sided page is two half page indices)
* return the index of the page that would appear on the right if you were to open the book to see
* that half page
*/
private int _HalfPageIdxToRightPageIdx(int halfPageIdx) {
    return (int)(Mathf.Floor((halfPageIdx - 1f) / 2f) + 1f);
}

/** 
* Updates the current open right page and clears any unseen flags
*/
private void _UpdateCurRightPage(int newIdx) {
    _curRightPageIdx = newIdx;

    // TODO Sets particle emission

    // TODO add unseen clearing


}

// Get Vector2 screen space coordinates of the object
private Vector2 _ScreenSpace2D(Transform obj, Camera cam) {
    Vector3 pos3D = cam.WorldToScreenPoint(obj.transform.position);
    return new Vector2(pos3D.x, pos3D.y);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// █▀ ▀█▀ ▄▀█ ▀█▀ █▀▀   ▀█▀ █▀█ ▄▀█ █▄░█ █▀ █ ▀█▀ █ █▀█ █▄░█ █▀
// ▄█ ░█░ █▀█ ░█░ ██▄   ░█░ █▀▄ █▀█ █░▀█ ▄█ █ ░█░ █ █▄█ █░▀█ ▄█
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Book is opening from closed icon form
    private bool _TransitionOpening() {
        if (_state == BookStates.CLOSED) {
            _state = BookStates.OPENING;
            _UpdateCurRightPage(_HalfPageIdxToRightPageIdx(_unseenContent[_unseenContent.Count - 1]));
            StartCoroutine(_OpenBook(_OpenCloseTime));
            _StopAllNodeParticleSystems();
            return true;
        }
        return false;
    }

    // Dragging of page has started, record start pos, set cursor to fist, set state
    private bool _TransitionTurn(bool isRight) {
        if (_state == BookStates.OPEN) {
            if (isRight) _state = BookStates.TURNING_LEFT;
            else _state = BookStates.TURNING_RIGHT;

            _mouseXWhenStartedDragging = _controls.Game.MousePos.ReadValue<Vector2>().x;
            Cursor.SetCursor(_cursorFist, new Vector2(50, 50), CursorMode.Auto);

            _StopAllNodeParticleSystems();

            return true;
        }
        return false;
    }

    // Book is open after opening transition, just set state
    private bool _TransitionOpen() {
        if (_state == BookStates.OPENING || _state == BookStates.AUTO_CLOSING) {
            _state = BookStates.OPEN;

            _ToggleParticle(true, _coverNodeParticleSys);
            if (_ExistsPage(_curRightPageIdx - 1)) _ToggleParticle(true, _leftNodeParticleSys);
            if (_ExistsPage(_curRightPageIdx)) _ToggleParticle(true, _rightNodeParticleSys);

            return true;
        } return false;
    }

    // Book is open again after flip, update page index and associated stuff
    private bool _TransitionOpen(bool isRight, bool didFlip) {
        if (_state == BookStates.FLIPPING) {
            _state = BookStates.OPEN;

            if (didFlip) {
                if (isRight) _UpdateCurRightPage(_curRightPageIdx + 1);
                else _UpdateCurRightPage(_curRightPageIdx - 1);
            }
            _ToggleParticle(true, _coverNodeParticleSys);
            if (_ExistsPage(_curRightPageIdx - 1)) _ToggleParticle(true, _leftNodeParticleSys);
            if (_ExistsPage(_curRightPageIdx)) _ToggleParticle(true, _rightNodeParticleSys);

            return true;
        } return false;
    }

    // Page is in auto-flip mode. Starts coroutine and sets cursor to open hand
    private bool _TransitionFlipping(float curPercent, bool isRight) {
        if (_state == BookStates.TURNING_LEFT || _state == BookStates.TURNING_RIGHT) {
            Cursor.SetCursor(_cursorOpenHand, new Vector2(50, 50), CursorMode.Auto);
            _state = BookStates.FLIPPING;
            StartCoroutine(_FlipPage(_flipPageTime, curPercent, isRight));
            _StopAllNodeParticleSystems();
            return true;
        } return false;
    }

    // Dragging cover to close has been released
    private bool _TransitionClosingAuto(float curClosedPercent) {
        if (_state == BookStates.CLOSING) {
            Cursor.SetCursor(_cursorOpenHand, new Vector2(50, 50), CursorMode.Auto);
            _state = BookStates.AUTO_CLOSING;
            StartCoroutine(_AutoClose(_flipPageTime*2, _OpenCloseTime, curClosedPercent));
            _StopAllNodeParticleSystems();
            return true;
        } return false;
    }

    // Dragging cover, record cursor, set fist, change state
    private bool _TransitionClosing() {
        if (_state == BookStates.OPEN) {
            _state = BookStates.CLOSING;

            _mouseXWhenStartedDragging = _controls.Game.MousePos.ReadValue<Vector2>().x;
            Cursor.SetCursor(_cursorFist, new Vector2(50, 50), CursorMode.Auto);
            _StopAllNodeParticleSystems();

            return true;
        }
        return false;
    }

    // Sets state to closed
    private bool _TransitionClosed() {
        if (_state == BookStates.AUTO_CLOSING) {
            _state = BookStates.CLOSED;
            Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
            _StopAllNodeParticleSystems();

            return true;
        }
        return false;
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// █▀ ▀█▀ ▄▀█ ▀█▀ █▀▀   █▀█ █▀█ █▀█ █▀▀ █▀▀ █▀ █▀ █ █▄░█ █▀▀
// ▄█ ░█░ █▀█ ░█░ ██▄   █▀▀ █▀▄ █▄█ █▄▄ ██▄ ▄█ ▄█ █ █░▀█ █▄█
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Update is called once per frame
    void Update() {
        _ProcessNewContentQueue();

        if (_state == BookStates.OPEN) _WhileOpen();
        else if (_state == BookStates.TURNING_LEFT) _WhileTurning(true);
        else if (_state == BookStates.TURNING_RIGHT) _WhileTurning(false);
        else if (_state == BookStates.CLOSING) _WhileClosing();
        else if (_state == BookStates.CLOSED) _WhileClosed();

        // Update mouse pressed
        _wasMouseDownLastFrame = _controls.Game.Fire.IsPressed();
    }

    // CLOSED STATE
    private void _WhileClosed() {
        Vector2 mousePos = _controls.Game.MousePos.ReadValue<Vector2>();

        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            Const.interp(_bookClosedRotation.y, _bookClosedRotation.y+5, 1-(mousePos.x / Screen.width)),
            Const.interp(_bookClosedRotation.z, _bookClosedRotation.z-5, mousePos.y / Screen.height)
        );

        bool withinRange = (_ScreenSpace2D(_iconBookCenter, _cam) - mousePos).magnitude <= Screen.height * _interactDistanceThreshold;

        if (withinRange) {
            
            if (_controls.Game.Fire.IsPressed()) {
                _TransitionOpening();
                Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
            } else {
                Cursor.SetCursor(_cursorOpenHand, new Vector2(50, 50), CursorMode.Auto);
            }
            
        } else {
            Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
        }
    }

    // OPENING STATE
    private IEnumerator _OpenBook(float timeToOpen) {
        float startTime = Time.time;

        while (true) {
            float percentDone = Const.SinEase(Mathf.Min((Time.time - startTime) / timeToOpen, 1));

            _SetBookOpenPositionPercent(percentDone);
            _SetBookOpenPercent(percentDone);

            if (Time.time - startTime >= timeToOpen) { break; }

            yield return null;
        }

        _TransitionOpen();
        yield return null;
    }

    // OPEN STATE
    private void _WhileOpen() {
        float z = (_cam.transform.position - _leftNode.transform.position).magnitude;
        Vector2 mousePos2D = _controls.Game.MousePos.ReadValue<Vector2>();

        bool isNearLeft = (_ScreenSpace2D(_leftNode, _cam) - mousePos2D).magnitude < Screen.height * _interactDistanceThreshold;
        bool isNearRight = (_ScreenSpace2D(_rightNode, _cam) - mousePos2D).magnitude < Screen.height * _interactDistanceThreshold;
        bool isNearCover = (_ScreenSpace2D(_coverNode, _cam) - mousePos2D).magnitude < Screen.height * _interactDistanceThreshold;

        if (isNearLeft || isNearCover || isNearRight) {
            Cursor.SetCursor(_cursorOpenHand, new Vector2(50, 50), CursorMode.Auto);

            if (!_wasMouseDownLastFrame && _controls.Game.Fire.IsPressed()) {
                if (isNearCover) _TransitionClosing();
                else if (isNearLeft && _ExistsPage(_curRightPageIdx - 1)) _TransitionTurn(false);
                else if (isNearRight && _ExistsPage(_curRightPageIdx)) _TransitionTurn(true);
            }

        } else {
            Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
        }
    }

    // CLOSING STATE: Dragging to close cover
    private void _WhileClosing() {
        float percentScreenDragged = (_controls.Game.MousePos.ReadValue<Vector2>().x - _mouseXWhenStartedDragging) / Screen.width;
        percentScreenDragged = Mathf.Max(percentScreenDragged, 0);
        float percentFlipped = Const.SinEase(Mathf.Min(percentScreenDragged / _dragScreenWidthPercentToTurnPage, 1));

        _SetBookOpenPercent(1-percentFlipped);

        if (!_controls.Game.Fire.IsPressed()) {
            _TransitionClosingAuto(percentFlipped);
        }
    }

    /** TURNING LEFT / RIGHT STATES: Controls dragging to turn page */
    private void _WhileTurning(bool isRight) {
        float percentScreenDragged = (_controls.Game.MousePos.ReadValue<Vector2>().x - _mouseXWhenStartedDragging) / Screen.width;

        if (isRight) { percentScreenDragged = Mathf.Abs(Mathf.Min(percentScreenDragged, 0)); }
        else { percentScreenDragged = Mathf.Abs(Mathf.Max(percentScreenDragged, 0)); }

        float percentFlipped = Const.SinEase(Mathf.Min(percentScreenDragged / _dragScreenWidthPercentToTurnPage, 1));

        _SetPageFlipPercent(isRight, percentFlipped);

        if (!_controls.Game.Fire.IsPressed()) {
            _TransitionFlipping(percentFlipped, isRight);
        }
    }

    /** Flips the current page over if curPercent > the commit percent, otherwise back to original state. */
    private IEnumerator _FlipPage(float timeToFlip, float curPercent, bool isRight) {
        float startTime = Time.time;
        if (curPercent >= _percentToCommitFlip) timeToFlip = timeToFlip * (1-curPercent);
        else timeToFlip = timeToFlip * curPercent;
        
        while (true) {
            float percent;
            if (curPercent >= _percentToCommitFlip) percent = curPercent + (Const.SinEase(Mathf.Min((Time.time - startTime) / timeToFlip, 1)) * (1-curPercent));
            else percent = 1-curPercent + (Const.SinEase(Mathf.Min((Time.time - startTime) / timeToFlip, 1)) * curPercent);

            if (curPercent >= _percentToCommitFlip) _SetPageFlipPercent(isRight, percent);
            else _SetPageFlipPercent(isRight, 1 - percent);

            if (Time.time - startTime >= timeToFlip) { break; }

            yield return null;
        }

        _TransitionOpen(isRight, curPercent >= _percentToCommitFlip);
        yield return null;
    }

    // CLOSING AUTO STATE: Release dragging, either opening back up or closing fully
    private IEnumerator _AutoClose(float timeToFlip, float timeToClose, float curPercentClosed) {
        float startTime = Time.time; 
        if (curPercentClosed > _percentToCommitFlip) timeToFlip = (1-curPercentClosed) * timeToFlip;
        else timeToFlip = curPercentClosed * timeToFlip;

        // Close/reopen book
        while (true) {
            float percent;
            if (curPercentClosed > _percentToCommitFlip) percent = curPercentClosed + (Const.SinEase(Mathf.Min((Time.time - startTime) / timeToFlip, 1)) * (1-curPercentClosed));
            else percent = 1-curPercentClosed + (Const.SinEase(Mathf.Min((Time.time - startTime) / timeToFlip, 1)) * curPercentClosed);

            if (curPercentClosed > _percentToCommitFlip) _SetBookOpenPercent(1-percent);
            else _SetBookOpenPercent(percent);

            if (Time.time - startTime >= timeToFlip) { break; }

            yield return null;
        }

        if (curPercentClosed <= _percentToCommitFlip) {
            _TransitionOpen();
            yield return null;
        }
        
        // If closed, move to icon corner
        else {
            startTime = Time.time;

            while (true) {
                float percent = Const.SinEase(Mathf.Min((Time.time - startTime) / timeToClose, 1));

                _SetBookOpenPositionPercent(1-percent);

                if (Time.time - startTime >= timeToClose) { break; }

                yield return null;
            }

            _TransitionClosed();
            yield return null;
        }
    }
}
