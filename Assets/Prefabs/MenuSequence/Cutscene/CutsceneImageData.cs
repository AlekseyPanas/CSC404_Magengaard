using UnityEngine;

public class CutsceneImageData : MonoBehaviour
{
    [HideInInspector] public Vector2 baseOffsetMaxRightTop;
    [HideInInspector] public Vector2 baseOffsetMinLeftBottom;
    [Tooltip("Where the image moves over the course of the cutscene duration")][SerializeField] public Vector2 displacement;

    void Start() {
        RectTransform rt = GetComponent<RectTransform>();
        baseOffsetMinLeftBottom = rt.offsetMin;
        baseOffsetMaxRightTop = rt.offsetMax;
    }
}
