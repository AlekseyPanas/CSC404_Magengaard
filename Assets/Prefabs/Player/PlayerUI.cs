using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    float hpPercent;
    [SerializeField] Image hpFill;
    [SerializeField] GameObject fadeToBlack;

    void Start(){
        PlayerHealthSystem.onTakedamage += UpdateHPBar;
        PlayerHealthSystem.onDeath += OnDeath;
    }

    void UpdateHPBar(PlayerHealthSystem phs){
        hpPercent = phs.GetHPPercent();
        hpFill.fillAmount = hpPercent;
    }

    void OnDeath(){
        Invoke("FadeScreenToBlack", 1);
    }

    void FadeScreenToBlack(){
        fadeToBlack.GetComponent<FadeToBlackPanel>().startFadingToBlack(2f);
    }
}
