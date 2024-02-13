using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimation : MonoBehaviour
{
    private int curFrame = 0;
    private int totalFrames = 0;

    [SerializeField] private float timeBetweenFrames;
    [SerializeField] private float fadeoutDelay;
    [SerializeField] private Sprite[] frames;  // All the frames to be used in the animation in order of animateIn

    // Start is called before the first frame update
    void Start() {
        // Computes total frames
        totalFrames = frames.Length;
        
        // Animate in immediately, animate out when menu progresses
        StartCoroutine(animateIn());
        MainMenuLogic.KeyPressedEvent += () => {StartCoroutine(animateOut(fadeoutDelay)); };
    }

    // Sets the image based on the frame
    void updateImage() { GetComponent<Image>().sprite = frames[curFrame]; }

    IEnumerator animateIn() {
        while (curFrame < totalFrames - 1) {
            curFrame++;
            updateImage();
            yield return new WaitForSeconds(timeBetweenFrames);
        }
    }

    IEnumerator animateOut(float delay = 0) {
        if (delay >= 0) { yield return new WaitForSeconds(delay); }
        while (curFrame > 0) {
            curFrame--;
            updateImage();
            yield return new WaitForSeconds(timeBetweenFrames);
        }
    }
}
