using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FadeToBlackPanel : MonoBehaviour
{
    public bool fadeInImmediately = false;
    private Image panelImage;

    // Start is called before the first frame update
    void Start() { 
        panelImage = GetComponent<Image>();

        if (fadeInImmediately) { 
            StartCoroutine(fadeToTransparent(1.2f)); 
        } 
    }

    // Fade out into black and switch to cutscene afterwards
    private IEnumerator fadeToBlack(float timeToFade) {
        float startTime = Time.time;
        float startAlpha = panelImage.color.a;
        float neededAlpha = 1 - startAlpha;

        while (panelImage.color.a < 1) {
            Color col = panelImage.color;
            float timePassed = Time.time - startTime;
            panelImage.color = new Color(col.r, col.g, col.b, startAlpha + (fadeCurve(timePassed / timeToFade) * neededAlpha));
            yield return null;    
        }
    }

    // Co-routine to fade from black into alpha
    private IEnumerator fadeToTransparent(float timeToFade) {
        float startTime = Time.time;
        float startAlpha = panelImage.color.a;
        float neededAlpha = startAlpha;

        while (panelImage.color.a > 0) {
            Color col = panelImage.color;
            float timePassed = Time.time - startTime;
            panelImage.color = new Color(col.r, col.g, col.b, startAlpha - (fadeCurve(timePassed / timeToFade, true) * neededAlpha));
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

    /** makes the alpha transition non-linear since for some reason its nonlinear in Unity */
    private float fadeCurve(float x, bool inverse = false) {
        return _fadeCurveCubic(x, inverse);
    }

    /** Give a value 0-1, converts to an exponential curve which creates a smoother fade */
    private float _fadeCurveExponential(float x, bool inverse = false) {
        if (!inverse) { return -Mathf.Pow(0.001f, x) + 1; }
        else { return Mathf.Pow(0.001f, -(x-1)); }
    }

    /** Give a value 0-1, converts to a cubic curve which creates a smoother fade */
    private float _fadeCurveCubic(float x, bool inverse = false) {
        if (!inverse) { return Mathf.Pow(x-1, 3) + 1; }
        else { return Mathf.Pow(x, 3); }
    }
}
