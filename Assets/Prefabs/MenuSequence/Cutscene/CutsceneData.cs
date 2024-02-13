using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneData : MonoBehaviour {
    [Tooltip("If true, the next cutscene will have a fade-to-black transition")][SerializeField] public bool isFadeNext;
    [Tooltip("How long this cutscene stays on screen")][SerializeField] public float durationsSeconds;
    [Tooltip("Sequential subtitle text")][SerializeField] public string[] subtitleTexts;
    [Tooltip("List of vectors corresponding to the above subtitles indicating the start time in .X and the end time in .Y")][SerializeField] public Vector2[] startEndTimes;
    [Tooltip("Child cutscene panel image objects")][SerializeField] public CutsceneImageData[] images;
}
