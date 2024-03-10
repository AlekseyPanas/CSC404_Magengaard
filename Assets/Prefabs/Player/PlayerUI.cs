using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    float hpPercent;
    [SerializeField] Image hpFill;
    [SerializeField] GameObject fadeToBlack;
    [SerializeField] List<Transform> energySegments;
    [SerializeField] List<GameObject> energyLevels;
    [SerializeField] Image energyTimerBar;
    public int totalSegments;
    public int currSegments;
    Camera energyCam;

    void Start(){
        PlayerHealthSystem.onTakedamage += UpdateHPBar;
        PlayerHealthSystem.onDeath += OnDeath;
        PlayerHealthSystem.onRespawn += FadeInFromBlack;
        PlayerHealthSystem.onRespawn += ResetHPBar;
        fadeToBlack.SetActive(true);
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

    void FadeInFromBlack(){
        fadeToBlack.GetComponent<FadeToBlackPanel>().startFadingToTransparent(2f);
    }

    void ResetHPBar(){
        hpFill.fillAmount = 1f;
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
        //ResetRenderTexture();
    }

    void OnTimeBarChange(float fill){
        energyTimerBar.fillAmount = fill;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            OnNewGesture(new Gesture(), 2, 4);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            OnEnergyChange(2);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            OnTimeBarChange(0.5f);
        }
    }

    void ResetRenderTexture(){
        RenderTexture targetTexture = energyCam.targetTexture;
        energyCam.Render();
    }
}
