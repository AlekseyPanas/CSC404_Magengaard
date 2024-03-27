using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

/** A Fallen page UI object listens for a pickupable, opens the UI when event received, and fires a finished inspection event when
image is closed  */
public class FallenPageUI : MonoBehaviour, IInspectable
{

    public static event Action<Sprite> PagePickedUpEvent = delegate { };  // Listen for this event to get page sprites

    private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;
    private ControllerRegistrant _registrant;
    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };

    [SerializeField] private EventReference _soundOpen;
    [SerializeField] private EventReference _soundClose;
    [SerializeField] private Texture2D _normalMap;  // can be null 
    [SerializeField] private float _heightPercentScreenHeight;  // height of open page relative to screen height
    [SerializeField] private float _widthPercentPageHeight;  // Width as percent of page height to preserve ratio
    [SerializeField] private Vector2 _vanishPosAnchorOffsetPercentWH;  // Position relative to anchor where page goes after fading out (specific as percent of width, percent of height)
    [SerializeField] private float _transitionTime;

    private SpellbookContributor _contributor;

    private StudioEventEmitter _emitterOpen;
    private StudioEventEmitter _emitterClose;

    private RectTransform _rt;

    private bool _isOpen = false;
    private bool _hasOpenedBefore = false;
    private bool _triggeredFullyOpen = false;
    private bool _triggeredFullyCLose = false;
    private float _curTransitionStartTime;

    private void _SetPositionPercentVanished(float percent) {
        _rt.anchoredPosition = new Vector2 (
            _vanishPosAnchorOffsetPercentWH.x * Screen.width * Const.SinEase(percent),
            _vanishPosAnchorOffsetPercentWH.y * Screen.height * Const.SinEase(percent)
        );
    }

    private void _SetScalePercentOpen(float percent) {
        float height = _heightPercentScreenHeight * Screen.height * Const.SinEase(percent); 
        float width = height * _widthPercentPageHeight;
        
        _rt.sizeDelta = new Vector2(width, height);
    }

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };

        _rt = GetComponent<RectTransform>();

        _emitterOpen = gameObject.AddComponent<StudioEventEmitter>();
        _emitterOpen.EventReference = _soundOpen;
        _emitterClose = gameObject.AddComponent<StudioEventEmitter>();
        _emitterClose.EventReference = _soundClose;

        _contributor = new SpellbookContributor();
    }

    void Start() {
        _contributor.Bake(GetComponent<Image>().sprite.texture, _normalMap);
        _SetScalePercentOpen(0);
    }


    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        if (!_hasOpenedBefore) {
            _isOpen = true;
            _hasOpenedBefore = true;
            _curTransitionStartTime = Time.time;

            _registrant = pickupRegistrant;
            _registrant.OnInterrupt = Close;

            _emitterOpen.Play();
        }        
    }

    void Close() {
        if (_isOpen) {
            _isOpen = false;
            _curTransitionStartTime = Time.time;
            _pickupSys.DeRegisterController(_registrant);
            PagePickedUpEvent(GetComponent<Image>().sprite);
        }
    }

    void Update() {
        float percent = Const.SinEase(Mathf.Min(1, (Time.time - _curTransitionStartTime) / _transitionTime));

        if (_isOpen && !_triggeredFullyOpen) {
            _SetScalePercentOpen(percent);

            if (percent >= 1) {
                _triggeredFullyOpen = true;
                OnFullyOpen();
            }
        }

        if (!_isOpen && _hasOpenedBefore && !_triggeredFullyCLose) {
            _SetScalePercentOpen(1-percent);
            _SetPositionPercentVanished(percent);

            if (percent >= 1) {
                _triggeredFullyCLose = true;
                OnFullyClose();
            }
        }
    }

    private void OnFullyOpen() {
        InputSystem.onAnyButtonPress.CallOnce(e => {
            Close();
            _emitterClose.Play();
        });
    }

    private void OnFullyClose() {
        _contributor.AddBakedContentToSpellbook();
    }
}