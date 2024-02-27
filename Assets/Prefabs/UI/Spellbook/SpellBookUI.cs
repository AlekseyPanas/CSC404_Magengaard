using System;
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
    [SerializeField] private Transform _ArrowClosetMouse;
    [SerializeField] private Transform _BookIconModel;
    [Tooltip("The Gesture System")]
    [SerializeField] private GestureSystem _gestSys;
    [Tooltip("Camera used by the gesture UI so that 3D UI objects can be drawn")]
    [SerializeField] private Camera _gestCamera;
    [SerializeField] private float _mouseIconLoopDuration;

    // Screen locations of book and arrows as a percentage of width and height (x,y) respectively
    private Vector2 _ArrowLeftRelScreenLoc = new(0.6f, 0.2f);
    private Vector2 _ArrowRightRelScreenLoc = new(0.4f, 0.2f);
    private Vector2 _ArrowCloseRelScreenLoc = new(0.8f, 0.8f);
    private Vector2 _BookModelRelScreenLoc = new(0.95f, 0.05f);
    private Vector2 _arrowLeftPurePos;
    private Vector2 _arrowRightPurePos;
    private Vector2 _arrowClosePurePos;
    private Vector2 _bookModelPurePos;
    private float _arrowMouseIconMoveDist;
    private float _distFromCam = 7;

    private bool isSpellbookInInventory = false;  // Set to true once the spellbook is first picked up
    private int _PickupablesListBookIndex = (int)PickupablesNetworkPrefabListIndexes.BOOK;  // prefab that the pickup system should spawn out of JJ's pocket

    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };  // Call this when the item is unpocketed

    /** Convert a 2D screen position from being a percentage of width, height to being a screen pixel coordinate */
    Vector2 ScreenPosFromRelative(Vector2 relativePos) {
        return new Vector2(relativePos.x * Screen.width, relativePos.y * Screen.height);
    }

    float ScreenDistToWorldDist(float screenDist, float distFromCam) {
        return (_gestCamera.ScreenToWorldPoint(new Vector3(0, 0, _distFromCam)) - _gestCamera.ScreenToWorldPoint(new Vector3(screenDist, 0, distFromCam))).magnitude;
    }

    void Start() {
        _arrowLeftPurePos = ScreenPosFromRelative(_ArrowLeftRelScreenLoc);
        _ArrowLeft.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowLeftPurePos.x, _arrowLeftPurePos.y, _distFromCam));
        _ArrowLeft.rotation = _gestCamera.transform.rotation;
        _ArrowLeft.Rotate(new Vector3(180, 90, 90));

        _arrowRightPurePos = ScreenPosFromRelative(_ArrowRightRelScreenLoc);
        _ArrowRight.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowRightPurePos.x, _arrowRightPurePos.y, _distFromCam));
        _ArrowRight.rotation = _gestCamera.transform.rotation;
        _ArrowRight.Rotate(new Vector3(0, 90, 90));

        _arrowClosePurePos = ScreenPosFromRelative(_ArrowCloseRelScreenLoc);
        _ArrowClose.position = _gestCamera.ScreenToWorldPoint(new Vector3(_arrowClosePurePos.x, _arrowClosePurePos.y, _distFromCam));
        _ArrowClose.rotation = _gestCamera.transform.rotation;
        _ArrowClose.Rotate(new Vector3(45, 90, 90));

        _bookModelPurePos = ScreenPosFromRelative(_BookModelRelScreenLoc);
        _BookIconModel.position = _gestCamera.ScreenToWorldPoint(new Vector3(_bookModelPurePos.x, _bookModelPurePos.y, _distFromCam));
        _BookIconModel.rotation = _gestCamera.transform.rotation;
        _BookIconModel.Rotate(new Vector3(0, 190, 0));

        _arrowMouseIconMoveDist = ScreenDistToWorldDist(Screen.height * 0.5f, _distFromCam);  
    }

    void Update () {
        float percentComplete = (float)(((Time.time % _mouseIconLoopDuration) / _mouseIconLoopDuration) * _arrowMouseIconMoveDist);

        _ArrowLeftMouse.position = _ArrowLeft.position - (_ArrowLeft.forward.normalized * percentComplete);
        var col =_ArrowLeftMouse.GetComponent<SpriteRenderer>().color;
        _ArrowLeftMouse.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0.5f - (0.5f * percentComplete));

    }

    public void OnInspectStart(Action OnInspectEnd) {
        // isOpen = true;
        // this.OnInspectEnd = OnInspectEnd;
    }

        // if (isOpen && Input.GetKeyDown(KeyCode.X)) {  // TODO REPLACE WITH GESTURE SYSTEM
        //     isOpen = false;
        //     OnInspectEnd();
        // } else if (!isOpen && Input.GetKeyDown(KeyCode.B)) {
        //     OnUnpocketInspectableEvent(_PickupablesListBookIndex, gameObject);
        // }
}
