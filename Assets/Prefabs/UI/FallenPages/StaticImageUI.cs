using UnityEngine;


/** Attaches to a UI Panel game object. implements behavior for opening a static image on screen and closing it. Plays a
scaling animation for opening and closing */
public abstract class StaticImageUI: MonoBehaviour {
    [SerializeField] private Vector2 _finalSizePercentOfScreen;  // Final image size as a percentage of height for both
    [SerializeField] private float _lerpFactor;  // Speed at which the image opens or closes. Every frame it lerps to target with this factor 
    private RectTransform _rt;
    protected bool isOpen = false;  // Change this bool in inheriting class to toggle the image
    private bool _onOpenTriggered = false;
    //private bool _onCloseTriggered = false;

    protected void Awake() {
        _rt = GetComponent<RectTransform>();
        _rt.sizeDelta = new Vector2(0, 0);
    }

    void Update() {
        if (isOpen) {
            var lerped = new Vector2(_finalSizePercentOfScreen.x * Screen.height, _finalSizePercentOfScreen.y * Screen.height);
            _rt.sizeDelta = Vector2.Lerp(_rt.sizeDelta, lerped, _lerpFactor);

            var percentToOpen = _rt.sizeDelta.magnitude / lerped.magnitude;
            if (percentToOpen > 0.98 && !_onOpenTriggered) {
                _onOpenTriggered = true;
                OnFullyOpen();
            }

            //_onCloseTriggered = false;
        } 
        
        else {
            _rt.sizeDelta = Vector2.Lerp(_rt.sizeDelta, new Vector2(0, 0), _lerpFactor);

            _onOpenTriggered = false;
        }

        UpdateBody();
    }

    /** 
    * Called when the UI is fully open on screen
    */
    protected abstract void OnFullyOpen();

    // /**   TODO: Implement OnFullyClosed
    // * Called when the UI is fully closed
    // */
    // protected virtual void OnFullyClosed() {}

    protected virtual void UpdateBody() {}
}
