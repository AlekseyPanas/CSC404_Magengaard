using UnityEngine;


/** Attaches to a UI Panel game object. implements behavior for opening a static image on screen and closing it. Plays a
scaling animation for opening and closing */
public abstract class StaticImageUI: MonoBehaviour {
    [SerializeField] private Vector2 _finalSizePercentOfScreen;  // Final image size as a percentage of height for both
    [SerializeField] private float _lerpFactor;  // Speed at which the image opens or closes. Every frame it lerps to target with this factor 
    private RectTransform _rt;
    protected bool isOpen = false;  // Change this bool in inheriting class to toggle the image

    void Awake() {
        _rt = GetComponent<RectTransform>();
        _rt.sizeDelta = new Vector2(0, 0);
    }

    void Update() {
        if (isOpen) {
            var lerped = new Vector2(_finalSizePercentOfScreen.x * Screen.height, _finalSizePercentOfScreen.y * Screen.height);
            _rt.sizeDelta = Vector2.Lerp(_rt.sizeDelta, lerped, _lerpFactor);
        } else {
            _rt.sizeDelta = Vector2.Lerp(_rt.sizeDelta, new Vector2(0, 0), _lerpFactor);
        }

        UpdateBody();
    }

    protected abstract void UpdateBody();
}
