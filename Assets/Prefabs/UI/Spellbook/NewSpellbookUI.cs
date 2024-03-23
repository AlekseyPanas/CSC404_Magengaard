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

    [SerializeField] public EventReference _openSoundPath;
    [SerializeField] public EventReference _closeSoundPath;    

    [SerializeField] private Transform _leftCoverBone;
    [SerializeField] private Transform _rightCoverBone;
    [SerializeField] private Transform _leftPagesEffector;
    [SerializeField] private Transform _rightPagesEffector;

    [SerializeField] private Vector3 _PageSpawnRelativePos;  // Position relative to book origin where pages are instantiated
    [SerializeField] private Vector2 _leftContentUVtopLeft;  // Topleft corner on the texture where the left page content starts
    [SerializeField] private Vector2 _leftContentWidthHeight;  // Width height of the left page content on the texture
    [SerializeField] private Vector2 _rightContentUVtopLeft;  // Same but for right page content
    [SerializeField] private Vector2 _rightContentWidthHeight;  // ^

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

    private List<Texture2D> _content = new();
    private List<Texture2D> _contentNormals = new();
    private List<bool> _seenContent = new();
    private int _curLeftPageIndex = 0;  // When book is open, tracks the index of the content on the left page (used for new page spawning)

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

private void _AddNewContent(Texture2D content, Texture2D contentNormal = null) {
    _content.Add(content);
    _contentNormals.Add(contentNormal);
    _seenContent.Add(false);
}

private BookPage _SpawnNewPage(int leftIndex, Vector3 relativeEffectorPos) {
    // Position and parent the page
    var obj = Instantiate(_bookPagePrefab, transform);
    obj.transform.position = _PageSpawnRelativePos;
    var bookPage = obj.GetComponent<BookPage>();
    bookPage.PageEffector.transform.position = transform.position + relativeEffectorPos;

    // Compute texture with content
    var rend = bookPage.PageMesh.GetComponent<MeshRenderer>();
    Texture2D tex = rend.material.GetTexture("_MainTex") as Texture2D;
    tex.SetPixels

    return bookPage;
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
