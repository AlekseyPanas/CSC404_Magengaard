using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SandEngraving : MonoBehaviour
{
    private DecalProjector _rend;
    private bool faded = false;

    // Start is called before the first frame update
    void Start() {
        _rend = GetComponent<DecalProjector>();
    }

    private IEnumerator fadeDecal(float timeToFade) {
        float startTime = Time.time;

        while (_rend.fadeFactor > 0) {
            _rend.fadeFactor = 1 - Math.Min((Time.time - startTime) / timeToFade, 1f);
            yield return null;
        }
        
    }

    /** Causes the decal's opacity to vanish within the given time */
    public void FadeOut(float timeToFadeSeconds) {
        if (!faded) {
            StartCoroutine(fadeDecal(timeToFadeSeconds));
            faded = true;
        }
    }
}
