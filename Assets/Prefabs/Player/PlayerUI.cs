using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] GameObject energyTimerBar;
    [SerializeField] GameObject blankGestureLineRenderer;
    [SerializeField] GameObject circle;
    [SerializeField] GameObject poofEnergyChange;
    [SerializeField] GameObject poofTimerExpire;
    [SerializeField] List<GameObject> binParticles;
    public int totalSegments;
    public int currSegments;
    public float particleSpawnInterval;
    int currEnergyLevel;
    [SerializeField] Animator anim;
    public List<GameObject> recordedGestures = new List<GameObject>();
    List<GameObject> spawnedEnergyBlocks = new List<GameObject>();
    
    float particleSpawnTimer;

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
            OnNewGesture(fireImpulseGest, 3, 4);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            List<GestComp> fireballGestComponents = new() { new GestComp(0, 1), new GestComp(-135, 2) };
            Gesture fireballGest = new(fireballGestComponents, 0.17f, -1);
            OnNewGesture(fireballGest, 4, 3);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)){
           OnEnergyChange(currSegments-1, false);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5)){
           OnTimeBarChange(0f);
        }
        if(Input.GetKeyDown(KeyCode.Alpha6)){
           ClearGestureQueue();
        }
        if (currEnergyLevel > 2) {
            SpawnBinParticles();
        }
    }
    
    /*
    Update energy levels, subscribe to the GestureSequenceAdd event
    */
    void OnNewGesture(Gesture g, int energyLevel, int segmentCount){
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetNumSegments(segmentCount);
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetFillAmount(1);
        foreach(Transform s in energySegments){
            s.gameObject.SetActive(false);
        }
        currEnergyLevel = energyLevel;
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
            energySegments[i].gameObject.SetActive(true);
            GameObject e = Instantiate(energyLevels[energyLevel-1], energySegments[i]);
            spawnedEnergyBlocks.Add(e);
        }
        currSegments = segmentCount;
    }
    void ClearGestureQueue(){
        foreach(GameObject g in recordedGestures){
            Destroy(g);
        }
        recordedGestures.Clear();
        UpdateGestureQueue();
    }
    void UpdateGestureQueue(){
        for(int i = recordedGestures.Count + 1; i < gesturePositions.Count; i++){
            gesturePositions[i].SetActive(false);
        }
        for (int i = 0; i < recordedGestures.Count; i++) {
            if (!gesturePositions[i].activeSelf){
                gesturePositions[i].SetActive(true);
                StartCoroutine(FadeInCircle(gesturePositions[i].GetComponent<SpriteRenderer>(), 0.5f));
            }
            GameObject gest = recordedGestures[i];
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
        g.transform.position = target;
    }
    IEnumerator ScaleTransitionSymbol(GameObject g, float startScale, float targetScale, float lerpTime){
        float timer = 0;
        while(timer < lerpTime){
            if (g == null) break;
            g.transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, timer/lerpTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        g.transform.localScale = Vector3.one * targetScale;
    }

    IEnumerator FadeInCircle(SpriteRenderer s, float lerpTime){
        s.color = new Color(1,1,1,0);
        float timer = 0;
        while(timer < lerpTime){
            if (s == null) break;
            Color c = s.color;
            float a = Mathf.Lerp(0, 1, timer/lerpTime);
            s.color = new Color(c.r, c.g, c.b, a);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    void SpawnBinParticles(){
        if (Time.time > particleSpawnTimer) {
            particleSpawnTimer = Time.time + particleSpawnInterval * Random.Range(0.8f, 1.2f);
            GameObject prefabToSpawn = binParticles[Random.Range(0, binParticles.Count)];
            Vector3 position = energySegments[Random.Range(0, currSegments)].transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-0.2f, 0.2f), 3);
            Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
    }

    void OnEnergyChange(int segmentsLeft, bool isBySwipe){
        GameObject poofType;
        if(isBySwipe){
            poofType = poofEnergyChange;
        } else {
            poofType = poofTimerExpire;
        }
        for (int i = currSegments; i > segmentsLeft; i--){
            Instantiate(poofType, energySegments[i-1].transform.position, Quaternion.identity);
            spawnedEnergyBlocks[i-1].GetComponent<EnergySegment>().OnDeplete();
        }
        currSegments = segmentsLeft;
    }

    void OnTimeBarChange(float fill){
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetFillAmount(fill);
    }


    // this doesnt work for some reason, the loop iterates twice even though it should only loop once. Probably something to do with coroutines
    
    // IEnumerator LoseEnergy(int n, GameObject poof){
    //     Debug.Log(currSegments - n);
    //     for (int i = currSegments; i > (currSegments - n); i--){
    //         Debug.Log(i);
    //         Instantiate(poof, energySegments[i-1].transform.position, Quaternion.identity);
    //         spawnedEnergyBlocks[i-1].GetComponent<EnergySegment>().OnDeplete();
    //         yield return new WaitForSeconds(0.3f);
    //     }
    // }

    void OnDestroy(){
        PlayerHealthControllable.OnHealthPercentChange -= UpdateHPBar;
        _deathSys.OnDeath -= OnDeath;
        PlayerDeathController.OnRespawn -= FadeInFromBlack;
        PlayerDeathController.OnRespawn -= ResetHPBar;
        PlayerCombatManager.OnEnterCombat -= ShowHUD;
        PlayerCombatManager.OnExitCombat -= HideHUD;
    }
}
