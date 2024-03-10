using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    IKillable _deathSys;
    [SerializeField] Image hpFill;
    [SerializeField] GameObject fadeToBlack;
    [SerializeField] List<Transform> energySegments;
    [SerializeField] List<GameObject> energyLevels;
    [SerializeField] Image energyTimerBar;
    public int totalSegments;
    public int currSegments;
    Camera energyCam;
    [SerializeField] Animator anim;

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform ply) => {
            _deathSys = ply.gameObject.GetComponent<IKillable>();
            _deathSys.OnDeath += OnDeath;
        };
    }

    void Start(){
        PlayerHealthControllable.OnHealthPercentChange += UpdateHPBar;
        PlayerDeathController.OnRespawn += FadeInFromBlack;
        PlayerDeathController.OnRespawn += ResetHPBar;
        PlayerCombatManager.OnEnterCombat += ShowHUD;
        PlayerCombatManager.OnExitCombat += HideHUD;
        fadeToBlack.SetActive(true);
    }

    void UpdateHPBar(float percentage, Vector3 dir){
        hpFill.fillAmount = percentage;
    }

    void OnDeath(GameObject gameObject) {
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
    
    /*
    Update energy levels, subscribe to the GestureSequenceAdd event
    */
    void OnNewGesture(Gesture g, int energyLevel, int segmentCount){

        for(int i = 0; i < segmentCount; i++){
            Instantiate(energyLevels[energyLevel], energySegments[i]);
        }
        currSegments = segmentCount;
        //figure out how to draw the gesture into the circle
    }

    void OnEnergyChange(int segmentsLeft){
        for (int i = currSegments; i > segmentsLeft; i--){
            Destroy(energySegments[i-1].GetChild(0).gameObject);
        }
        currSegments = segmentsLeft;
    }

    void OnTimeBarChange(float fill){
        energyTimerBar.fillAmount = fill;
    }

    void OnDestroy(){
        PlayerHealthControllable.OnHealthPercentChange -= UpdateHPBar;
        _deathSys.OnDeath -= OnDeath;
        PlayerDeathController.OnRespawn -= FadeInFromBlack;
        PlayerDeathController.OnRespawn -= ResetHPBar;
        PlayerCombatManager.OnEnterCombat -= ShowHUD;
        PlayerCombatManager.OnExitCombat -= HideHUD;
    }
}
