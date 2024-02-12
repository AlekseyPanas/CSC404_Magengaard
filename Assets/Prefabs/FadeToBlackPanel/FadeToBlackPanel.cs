using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FadeToBlackPanel : MonoBehaviour
{
    [SerializeField] private float timeSecondsBetweenFadeIncrements = 0.01f;

    // Start is called before the first frame update
    void Start() {
        StartCoroutine("fadeToTransparent");
    }

    // Fade out into black and switch to cutscene afterwards
    private IEnumerator fadeToBlack() {
        while (GetComponent<Image>().color.a < 1) {
            Color col = GetComponent<Image>().color;
            GetComponent<Image>().color = new Color(col.r, col.g, col.b, col.a + 0.01f);
            yield return new WaitForSeconds(timeSecondsBetweenFadeIncrements);    
        }
    }

    // Co-routine to fade from black into alpha
    private IEnumerator fadeToTransparent() {
        while (GetComponent<Image>().color.a > 0) {
            Color col = GetComponent<Image>().color;
            GetComponent<Image>().color = new Color(col.r, col.g, col.b, col.a - 0.01f);
            yield return new WaitForSeconds(timeSecondsBetweenFadeIncrements);    
        }
    }

    public void startFadingToBlack() {
        StopCoroutine("fadeToTransparent");
        StartCoroutine("fadeToBlack");
    }

    public void startFadingToTransparent () {
        StopCoroutine("fadeToBlack");
        StartCoroutine("fadeToTransparent");
    }
}
