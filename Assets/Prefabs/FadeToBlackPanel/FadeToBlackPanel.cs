using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FadeToBlackPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {}

    // Fade out into black and switch to cutscene afterwards
    private IEnumerator fadeToBlack(float timeToFade) {
        float startTime = Time.time;
        float startAlpha = GetComponent<Image>().color.a;
        float neededAlpha = 1 - startAlpha;

        while (GetComponent<Image>().color.a < 1) {
            Color col = GetComponent<Image>().color;
            float timePassed = Time.time - startTime;
            GetComponent<Image>().color = new Color(col.r, col.g, col.b, startAlpha + (fadeCurve(timePassed / timeToFade) * neededAlpha));
            yield return null;    
        }
    }

    // Co-routine to fade from black into alpha
    private IEnumerator fadeToTransparent(float timeToFade) {
        float startTime = Time.time;
        float startAlpha = GetComponent<Image>().color.a;
        float neededAlpha = startAlpha;

        while (GetComponent<Image>().color.a > 0) {
            Color col = GetComponent<Image>().color;
            float timePassed = Time.time - startTime;
            GetComponent<Image>().color = new Color(col.r, col.g, col.b, startAlpha - (fadeCurve(timePassed / timeToFade, true) * neededAlpha));
            yield return null;    
        }
    }

    public void startFadingToBlack(float timeToFade) {
        StopCoroutine("fadeToTransparent");
        StartCoroutine("fadeToBlack", timeToFade);
    }

    public void startFadingToTransparent (float timeToFade) {
        StopCoroutine("fadeToBlack");
        StartCoroutine("fadeToTransparent", timeToFade);
    }

    /** Give a value 0-1, converts to an exponential curve which creates a smoother fade */
    private float fadeCurve(float x, bool inverse = false) {
        if (!inverse) { return -Mathf.Pow(0.001f, x) + 1; }
        else { return Mathf.Pow(0.001f, -(x-1)); }
    }
}
