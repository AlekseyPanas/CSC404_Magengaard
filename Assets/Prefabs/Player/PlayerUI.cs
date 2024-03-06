using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Image hpFill;
    [SerializeField] GameObject fadeToBlack;
    [SerializeField] Animator anim;

    void Start(){
        PlayerHealthSystem.OnHealthPercentChange += UpdateHPBar;
        PlayerDeathController.OnDeath += OnDeath;
        PlayerDeathController.OnRespawn += FadeInFromBlack;
        PlayerDeathController.OnRespawn += ResetHPBar;
        PlayerCombatManager.enterCombat += ShowHUD;
        PlayerCombatManager.exitCombat += HideHUD;
        fadeToBlack.SetActive(true);
    }

    void UpdateHPBar(float percentage, Vector3 dir){
        hpFill.fillAmount = percentage;
    }

    void OnDeath(){
        Invoke("FadeScreenToBlack", 1);
    }

    void FadeScreenToBlack(){
        fadeToBlack.GetComponent<FadeToBlackPanel>().startFadingToBlack(2f);
    }

    void FadeInFromBlack(){
        fadeToBlack.GetComponent<FadeToBlackPanel>().startFadingToTransparent(2f);
    }

    void ResetHPBar(){
        hpFill.fillAmount = 1f;
    }

    void ShowHUD(){
        anim.SetTrigger("ShowHUD");
    }

    void HideHUD(){
        anim.SetTrigger("HideHUD");
    }
    
    void OnDestroy(){
        PlayerHealthSystem.OnHealthPercentChange -= UpdateHPBar;
        PlayerDeathController.OnDeath -= OnDeath;
        PlayerDeathController.OnRespawn -= FadeInFromBlack;
        PlayerDeathController.OnRespawn -= ResetHPBar;
        PlayerCombatManager.enterCombat -= ShowHUD;
        PlayerCombatManager.exitCombat -= HideHUD;
    }
}
