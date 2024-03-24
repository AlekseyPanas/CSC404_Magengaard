using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;


public class NewSpellbookUI : MonoBehaviour, IInspectable {

    enum BookStates {
        CLOSED = 0,
        OPENING = 1
    }

    [Tooltip("Camera used by the spellbook UI")] [SerializeField] private Camera _cam;
    [SerializeField] private GameObject _bookPagePrefab;
    [SerializeField] private Texture2D _pageBaseTexture;

    [SerializeField] public EventReference _openSoundPath;
    [SerializeField] public EventReference _closeSoundPath;    

    [SerializeField] private Transform _leftCoverBone;
    [SerializeField] private Transform _rightCoverBone;
    [SerializeField] private Transform _leftPagesEffector;
    [SerializeField] private Transform _rightPagesEffector;

    [SerializeField] private Vector3 _PageSpawnRelativePos;  // Position relative to book origin where pages are instantiated

    [SerializeField] private Vector2 _leftContentUVbotLeft;  // Bottom left corner on the texture where the left page content starts
    [SerializeField] private Vector2 _leftContentWidthHeight;  // Width height of the left page content on the texture
    [SerializeField] private Vector2 _rightContentUVbotLeft;  // Same but for right page content
    [SerializeField] private Vector2 _rightContentWidthHeight;  // ^

    // As the page flips, it goes from the left cover, through the middle, to the right cover. These anchors guide the animation
    [SerializeField] private Transform _leftCoverCurveAnchor;
    [SerializeField] private Transform _rightCoverCurveAnchor;
    [SerializeField] private Transform _middleCurveAnchor;

    // x-axis rotation values for the cover control bones when in fully closed or fully open configuration
    [SerializeField] private float _leftCoverClosedRotation;
    [SerializeField] private float _leftCoverOpenRotation;
    [SerializeField] private float _rightCoverClosedRotation;
    [SerializeField] private float _rightCoverOpenRotation;

    // When book is an icon, cover faces you. When book is open, pages face you. These are the two rotation endpoints for this
    [SerializeField] private float _bookClosedRotation;
    [SerializeField] private float _bookOpenRotation;

    private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;
    private ControllerRegistrant _pickupSysRegistrant;
    private DesktopControls _controls;
    private StudioEventEmitter _emitterOpen;
    private StudioEventEmitter _emitterClose;

    private bool _isSpellbookInInventory = false;  // Set to true once the spellbook is first picked up
    private BookStates _state = BookStates.CLOSED;

    private BookPage _leftBackupPg;
    private BookPage _leftPg;
    private BookPage _rightBackupPg;
    private BookPage _rightPg;

    private List<Texture2D> _content = new();  // List of computed textures for the pages (includes both sides, this is changed dynamically)
    private List<Texture2D> _contentNormals = new();  // Same as above but for normals
    private bool _prevFilled = true;  // previous content texture full, new content generates a new texture. Otherwise adds to right side
    private List<int> _unseenContent = new();  // List of indexes of unseen pages (one index per half of content texture, i.e one idx per page)
    private int _curLeftPageIndex = 0;  // When book is open, tracks the index of the content on the left page (used for new page spawning)


    [SerializeField] private Texture2D _testContent1;
    [SerializeField] private Texture2D _testContent2;
    [SerializeField] private Texture2D _testContent3;

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };

        _emitterOpen = gameObject.AddComponent<StudioEventEmitter>();
        _emitterOpen.EventReference = _openSoundPath;
        _emitterClose = gameObject.AddComponent<StudioEventEmitter>();
        _emitterClose.EventReference = _closeSoundPath;

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        // Enable input system
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        _AddNewContent(_testContent1);
        _AddNewContent(_testContent2);
        _AddNewContent(_testContent3);
        _SpawnNewPage(0, 1);

        Debug.Log(_rightCoverClosedRotation);
        _leftCoverBone.localEulerAngles = new Vector3(_leftCoverClosedRotation, _leftCoverBone.localEulerAngles.y, _leftCoverBone.localEulerAngles.z);
        _rightCoverBone.localEulerAngles = new Vector3(_rightCoverClosedRotation, _rightCoverBone.localEulerAngles.y, _rightCoverBone.localEulerAngles.z);
    }

    // Update is called once per frame
    void Update() {
        
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

/** 
* Scale the given texture on the GPU to the new size
*/
private Texture2D _RescaleTexture(Texture2D texture2D, int targetX, int targetY) {
    RenderTexture rt = new RenderTexture(targetX, targetY, 24);
    RenderTexture.active = rt;
    Graphics.Blit(texture2D, rt);
    Texture2D result = new Texture2D(targetX, targetY);
    result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
    result.Apply();
    return result;
}

/** 
* Blit src onto dest with the given offset including alphas using expensive CPU-side SetPixel ops
*/
private void CPUAlphaBlit(Texture2D src, Vector2 offset, Texture2D dest) {
    // Looks expensive af, hoping its not noticeable given how rarely this runs
    // Alpha blits the new scaled content onto newContentBase. Graphics.blit sadly doesnt support alphas hence this CPU side monstrosity
    for (int x = 0; x < src.width; x++) {
        for (int y = 0; y < src.height; y++) {
            Color pixSrc = src.GetPixel(x, y);

            if (pixSrc.a > 0) {
                Color destSrc = dest.GetPixel((int)(offset.x + x), (int)(offset.y + y));

                Color blendedCol = (pixSrc * pixSrc.a) + (destSrc * (1 - pixSrc.a));
                blendedCol.a = 1;

                dest.SetPixel((int)(offset.x + x), (int)(offset.y + y), blendedCol);
            }
        }
    } dest.Apply();
}

/** 
* Add one new page side of content into the spellbook
*/
private void _AddNewContent(Texture2D content, Texture2D contentNormal = null) {
    // Make a new page texture and populate the left side with new content
    if (_prevFilled) {
        _prevFilled = false;

        Texture2D newContentBase = new Texture2D(_pageBaseTexture.width, _pageBaseTexture.height);
        Graphics.CopyTexture(_pageBaseTexture, 0, 0, newContentBase, 0, 0);
        
        Texture2D scaledNewContent = _RescaleTexture(content, (int)_leftContentWidthHeight.x, (int)_leftContentWidthHeight.y);

        CPUAlphaBlit(scaledNewContent, _leftContentUVbotLeft, newContentBase);
        
        _content.Add(newContentBase);

        _unseenContent.Add(_content.Count * 2 - 2);
    } 

    // Take the last content (which has unpopulated right side) and blit to right side
    else {
        _prevFilled = true;

        Texture2D scaledNewContent = _RescaleTexture(content, (int)_rightContentWidthHeight.x, (int)_rightContentWidthHeight.y);

        CPUAlphaBlit(scaledNewContent, _rightContentUVbotLeft, _content[_content.Count - 1]);

        _unseenContent.Add(_content.Count * 2 - 1);
    }
    //_contentNormals.Add(contentNormal);   TODO: Add the same functionality as above but for normals
}

/** 
* Spawn a new spell page into the book textured with specified content index and bent x along
* the page flip effector curve (as calculated by _GetPageFlipPosition)
*/
private BookPage _SpawnNewPage(int contentIdx, double x) {
    // Position and parent the page
    var obj = Instantiate(_bookPagePrefab, transform);
    obj.transform.localPosition = _PageSpawnRelativePos;
    var bookPage = obj.GetComponent<BookPage>();
    Vector2 effectorXZ = _GetPageFlipPosition(x, 1);
    bookPage.PageEffector.transform.position = new Vector3(effectorXZ.x, bookPage.PageEffector.transform.position.y, effectorXZ.y);

    // Set texture
    var rend = bookPage.PageMesh.GetComponent<Renderer>();
    if (contentIdx >= _content.Count) {
        rend.material.SetTexture("_BaseMap", _pageBaseTexture);
    } else {
        rend.material.SetTexture("_BaseMap", _content[contentIdx]);
    }
    
    return bookPage;
}

/** 
* Curve for smoothly turning the page between the anchors
*/
private double _PageFlipCurve(double x) {
    return -0.55 * Math.Tanh(3 * (x - 0.5)) + 0.5;
}

/** 
* x is an input along the axis between the left and right cover anchors.
* x=0 is the point on that axis where the middle anchor projects onto it
* x=-1 is the left anchor
* x=1 is the right anchor
* This returns a world space vector2 xz plane position of where an effector should go
* Scale allows specifying a left-right anchor axis scale modifier
*/
private Vector2 _GetPageFlipPosition(double x, float scale) {
    // Project anchors to xz
    Vector2 left = new Vector2(_leftCoverCurveAnchor.position.x, _leftCoverCurveAnchor.position.z);
    Vector2 right = new Vector2(_rightCoverCurveAnchor.position.x, _rightCoverCurveAnchor.position.z);
    Vector2 middle = new Vector2(_middleCurveAnchor.position.x, _middleCurveAnchor.position.z);

    // Get positive direction along left-to-right anchor axis
    Vector2 xPosDir = (right - left).normalized;

    // Find the projection of middle anchor onto left-to-right anchor axis. Use that to get y direction
    float projection_mag = Vector2.Dot(middle - left, xPosDir);
    Vector2 proj = left + (projection_mag * xPosDir);
    Vector2 yDir = middle - proj;

    Vector2 xPosFull = right - proj;
    Vector2 xNegFull = left - proj;

    // Use page curve to get amount along orthogonal "y" direction, combine with x direction
    if (x > 0) {
        return proj + (Mathf.Abs((float)x) * scale * xPosFull) + ((float)_PageFlipCurve(Mathf.Abs((float)x)) * yDir);
    } else {
        return proj + (Mathf.Abs((float)x) * scale * xNegFull) + ((float)_PageFlipCurve(Mathf.Abs((float)x)) * yDir);
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// █▀ ▀█▀ ▄▀█ ▀█▀ █▀▀   ▀█▀ █▀█ ▄▀█ █▄░█ █▀ █ ▀█▀ █ █▀█ █▄░█ █▀
// ▄█ ░█░ █▀█ ░█░ ██▄   ░█░ █▀▄ █▀█ █░▀█ ▄█ █ ░█░ █ █▄█ █░▀█ ▄█
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    /** 
    * Start opening transition if able, return if started
    */
    private bool _TransitionOpening() {
        if (_state == BookStates.CLOSED) {
            _state = BookStates.OPENING;
            StartCoroutine(_OpenBook(3));
            return true;
        }
        return false;
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// █▀ ▀█▀ ▄▀█ ▀█▀ █▀▀   █▀█ █▀█ █▀█ █▀▀ █▀▀ █▀ █▀ █ █▄░█ █▀▀
// ▄█ ░█░ █▀█ ░█░ ██▄   █▀▀ █▀▄ █▄█ █▄▄ ██▄ ▄█ ▄█ █ █░▀█ █▄█
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    private IEnumerator _OpenBook(float timeToOpen) {
        yield return null;
    }
}
