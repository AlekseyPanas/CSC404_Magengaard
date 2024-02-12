using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlashingText : MonoBehaviour
{

    [SerializeField] private float flashSpeed = 3;
    
    bool hasPressedKey = false;
    float cur_alpha = 1;

    // Start is called before the first frame update
    void Start() {
        MainMenuLogic.KeyPressedEvent += () => { 
            hasPressedKey = true;
            StartCoroutine(fadeTextAway());
        };
    }

    // Flashing text effect
    void Update() {
        if (!hasPressedKey) {
            cur_alpha = (Mathf.Sin(Time.time * flashSpeed) + 1) * 0.5f;
            _changeAlpha(cur_alpha);
        }
    }

    // Fade out the text
    IEnumerator fadeTextAway() {
        while (cur_alpha > 0) {
            cur_alpha -= 0.02f;
            _changeAlpha(cur_alpha);
            yield return new WaitForSeconds(0.02f);
        }
    }

    void _changeAlpha(float a) {
        Color col = GetComponent<TextMeshProUGUI>().color;
        GetComponent<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, a);
    }
}
