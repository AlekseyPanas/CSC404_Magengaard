using System.Collections;
using TMPro;
using UnityEngine;

/** Controls the entire cutscene sequence on screen by reading the cutscenes list and sequentially executing each one according to the parameters */
public class CutsceneManager : AWaitFor
{
    [Tooltip("Sequential subtitle text")][SerializeField] public string[] subtitleTexts;
    [Tooltip("List of vectors corresponding to the above subtitles indicating the start time in .X and the end time in .Y")][SerializeField] public Vector2[] startEndTimes;
    [SerializeField] private float timeToFadePanel = 1.2f;
    [SerializeField] private FadeToBlackPanel fadeToBlackPanel;
    [SerializeField] private CutsceneData[] cutscenes;
    [SerializeField] private TextMeshProUGUI subtitles;
    
    private int curCutsceneIdx = 0;

    // Start is called before the first frame update
    void Start() {
        foreach (CutsceneData c in cutscenes) { c.gameObject.SetActive(false); }
        cutscenes[curCutsceneIdx].gameObject.SetActive(true);

        // Start first cutscene and start subtitles
        StartCoroutine(executeScene());
        StartCoroutine(executeSubtitles());

        
    }

    /** increment cut scene index and start next cutscene coroutine */
    void changeCutscene() {
        if (curCutsceneIdx < cutscenes.Length - 1) {
            cutscenes[curCutsceneIdx].gameObject.SetActive(false);
            curCutsceneIdx++;
            cutscenes[curCutsceneIdx].gameObject.SetActive(true);
            StartCoroutine(executeScene());
        } else {
            // TODO: Tell scene changer to enter the game
            invokeFinishedTask();
        }
    }   

    IEnumerator executeScene() {
        // Record start time, fade in the panel
        float startTime = Time.time;
        float duration = cutscenes[curCutsceneIdx].durationsSeconds;
        fadeToBlackPanel.startFadingToTransparent(timeToFadePanel);
        bool startedFadeout = false;

        // Loop while duration is not expired
        while (Time.time - startTime <= duration) {
            // Compute progress within this cutscene
            float timePassed = Time.time - startTime;
            float timeLeft = duration - timePassed;

            // Move each of the images by the percentage of their configured displacement
            foreach (CutsceneImageData imdat in cutscenes[curCutsceneIdx].images) {
                RectTransform rt = imdat.gameObject.GetComponent<RectTransform>();

                Vector2 curDisp = (timePassed / duration) * imdat.displacement;
                rt.offsetMin = imdat.baseOffsetMinLeftBottom + curDisp;
                rt.offsetMax = imdat.baseOffsetMaxRightTop + curDisp;
            }

            // In the last second, fade to black if the next scene is supposed to fade
            if (timeLeft <= 1f && !startedFadeout && cutscenes[curCutsceneIdx].isFadeNext) { 
                fadeToBlackPanel.startFadingToBlack(timeToFadePanel); 
                startedFadeout = true;
            }

            yield return null;
        }

        // Change to the next cutscene
        changeCutscene();
    }

    IEnumerator executeSubtitles() {
        float startTime = Time.time;  // Record initial time
        
        // Compute the latest subtitle finish time. This is when the coroutine should stop
        float latestEndTime = 0;
        foreach (Vector2 startEndTime in startEndTimes) {
            if (startEndTime.y > latestEndTime) { latestEndTime = startEndTime.y; }
        }

        int subtitleIndex = -1;  // Prevents quitting before text can disable

        // While still more subtitles...
        while (Time.time - startTime <= latestEndTime || subtitleIndex != -1) {
            float curTime = Time.time - startTime;

            // Compute if any subtitle needs to be showing right now
            subtitleIndex = -1;
            for (int i = 0; i < startEndTimes.Length; i++) {
                if (curTime <= startEndTimes[i].y && curTime >= startEndTimes[i].x) {
                    subtitleIndex = i;
                    break;
                }
            }

            // Depending on which subtitle needs to be shown, show it, or otherwise disable text
            if (subtitleIndex != -1) {
                subtitles.SetText(subtitleTexts[subtitleIndex]);
            } else {
                subtitles.SetText("");
            }

            yield return null;
        }
        
    }
}
