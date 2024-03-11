using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] List<GameObject> gesturePositions;
    [SerializeField] Image energyTimerBar;
    [SerializeField] GameObject blankGestureLineRenderer;
    [SerializeField] GameObject circle;
    public int totalSegments;
    public int currSegments;
    [SerializeField] Animator anim;
    public List<GameObject> recordedGestures = new List<GameObject>();
    List<GameObject> spawnedEnergyBlocks = new List<GameObject>();

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

    void Update(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            List<GestComp> windImpulseGestComponents = new() { new GestComp(45, 1), new GestComp(-90, 1), new GestComp(90, 1) };
            Gesture windImpulseGest = new(windImpulseGestComponents, 0.17f, -1);
            OnNewGesture(windImpulseGest, 2, 4);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            List<GestComp> fireImpulseGestComponents = new() { new GestComp(110, 2), new GestComp(140, 2), new GestComp(70, 1) };
            Gesture fireImpulseGest = new(fireImpulseGestComponents, 0.17f, -1);
            OnNewGesture(fireImpulseGest, 2, 4);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            List<GestComp> fireballGestComponents = new() { new GestComp(0, 1), new GestComp(-135, 2) };
            Gesture fireballGest = new(fireballGestComponents, 0.17f, -1);
            OnNewGesture(fireballGest, 2, 3);
        }
    }
    
    /*
    Update energy levels, subscribe to the GestureSequenceAdd event
    */
    void OnNewGesture(Gesture g, int energyLevel, int segmentCount){
        GameObject newGestureLine = Instantiate(blankGestureLineRenderer, circle.transform);
        newGestureLine.GetComponent<GestureLineRenderer>().SetGesture(g);
        recordedGestures.Insert(0, newGestureLine);
        if (recordedGestures.Count > gesturePositions.Count){
            GameObject lastGesture = recordedGestures[recordedGestures.Count-1];
            recordedGestures.Remove(lastGesture);
            Destroy(lastGesture);
        }
        UpdateGestureQueue();

        foreach(GameObject e in spawnedEnergyBlocks){
            Destroy(e);
        }
        spawnedEnergyBlocks.Clear();
        for(int i = 0; i < segmentCount; i++){
            GameObject e = Instantiate(energyLevels[energyLevel-1], energySegments[i]);
            spawnedEnergyBlocks.Add(e);
        }
        currSegments = segmentCount;
    }

    void UpdateGestureQueue(){
        List<GameObject> clone = recordedGestures.ToList();
        for (int i = 0; i < recordedGestures.Count; i++) {
            GameObject gest = clone[i];
            LineRenderer lr = gest.GetComponent<LineRenderer>();
            StartCoroutine(MoveGestureSymbol(gest, gest.transform.position, gesturePositions[i].transform.position, 0.5f));
            if(i == 0){
                StartCoroutine(ScaleTransitionSymbol(gest, 0.4f, 0.4f, 0.5f));
            } else if (i == 1) {
                StartCoroutine(ScaleTransitionSymbol(gest, 0.4f, 0.2f, 0.5f));
            } else if (i == gesturePositions.Count - 1) {
                StartCoroutine(ScaleTransitionSymbol(gest, 0.2f, 0.0f, 0.5f));
            } else {
                StartCoroutine(ScaleTransitionSymbol(gest, 0.2f, 0.2f, 0.5f));
            }
            Gradient g = lr.colorGradient;
            float alpha = 1 - i * (1f / gesturePositions.Count);
            g.SetKeys( g.colorKeys,  new GradientAlphaKey[] { new(alpha, 0.0f), new(alpha, 1.0f) } );
            lr.colorGradient = g;
        }
    }

    IEnumerator MoveGestureSymbol(GameObject g, Vector3 start, Vector3 target, float lerpTime){
        float timer = 0;
        while(timer < lerpTime){
            if (g == null) break;
            g.transform.position = Vector3.Lerp(start, target, timer/ lerpTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator ScaleTransitionSymbol(GameObject g, float startScale, float targetScale, float lerpTime){
        float timer = 0;
        while(timer < lerpTime){
            if (g == null) break;
            g.transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, timer/lerpTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
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
