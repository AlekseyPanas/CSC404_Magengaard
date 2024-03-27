using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] Transform magicCirclePosition;
    [SerializeField] GameObject magicCircle;
    [SerializeField] Animator anim;
    [SerializeField] Animator spellEnergyAnim;
    [SerializeField] Camera cam;
    [SerializeField] GameObject spellEnergyUIElements;
    [SerializeField] GameObject fireEnergySegment;
    [SerializeField] GameObject iceEnergySegment;
    [SerializeField] GameObject windEnergySegment;
    [SerializeField] GameObject lightningEnergySegment;
    [SerializeField] GameObject gestureCircle;
    [SerializeField] Transform screenCenter;
    [SerializeField] GameObject newSegmentParticles;
    [SerializeField] AnimationCurve movementCurve;
    public int totalSegments;
    public int currSegments;
    public float particleSpawnInterval;
    int currEnergyLevel;
    public List<GameObject> recordedGestures = new List<GameObject>();
    List<GameObject> spawnedEnergyBlocks = new List<GameObject>();
    GameObject spawnedMagicCircle = null;
    float particleSpawnTimer;
    SpellElementColorPalette currColorPalette;

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform ply) => {
            _deathSys = ply.gameObject.GetComponent<IKillable>();
            _deathSys.OnDeath += OnDeath;
        };

        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += (a, b) => {
            fadeToBlack.GetComponent<FadeToBlackPanel>().startFadingToTransparent(1);
        };
    }

    void Start(){
        PlayerHealthControllable.OnHealthPercentChange += UpdateHPBar;
        PlayerDeathController.OnRespawn += FadeInFromBlack;
        PlayerDeathController.OnRespawn += ResetHPBar;
        PlayerCombatManager.OnEnterCombat += ShowHUD;
        PlayerCombatManager.OnExitCombat += HideHUD;
        SpellSystem.GestureSequenceClearEvent += ClearGestureQueue;
        SpellSystem.GestureSequenceAddEvent += OnNewGesture;
        SpellSystem.SetSegmentFillEvent += OnEnergyChange;
        SpellSystem.SetTimerBarPercentEvent += OnTimeBarChange;

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

    void ShowSpellEnergyUI(){
        spellEnergyAnim.SetBool("ShowSpellEnergyUI", true);
    }
    void HideSpellEnergyUI(){
        spellEnergyAnim.SetBool("ShowSpellEnergyUI", false);
    }

    void Update(){
        if (currEnergyLevel > 2 && currSegments > 0) {
            SpawnBinParticles();
        }
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        spellEnergyUIElements.transform.position = new Vector3(topRight.x - 10, topRight.y - 5, transform.position.z);

        //testing
        if (Input.GetKeyDown(KeyCode.Space)){
            OnNewCapsule(0, new FirePalette());
        }
    }
    
    /*
    Update energy levels, subscribe to the GestureSequenceAdd event
    */
    void OnNewGesture(Gesture g, GestureBinNumbers gestureBinNumber, int segmentCount, SpellElementColorPalette colorPalette){
        currColorPalette = colorPalette;
        
        foreach(ParticleSystem cp in magicCircle.transform.GetComponentsInChildren<ParticleSystem>()){
            var cp_main = cp.main;
            cp_main.startColor = currColorPalette.GetNaryColor(0);
        }

        int energyLevel = (int)gestureBinNumber;
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetNumSegments(segmentCount);
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetFillAmount(1);
        foreach(Transform s in energySegments){
            s.gameObject.SetActive(false);
        }
        currEnergyLevel = energyLevel;
        GameObject newGestureLine = Instantiate(blankGestureLineRenderer, circle.transform);
        newGestureLine.GetComponent<GestureLineRenderer>().SetGesture(g);
        newGestureLine.GetComponent<GestureLineRenderer>().SetColor(colorPalette.GetNaryColor(0));
        recordedGestures.Insert(0, newGestureLine);
        if (recordedGestures.Count > gesturePositions.Count){
            GameObject lastGesture = recordedGestures[recordedGestures.Count-1];
            recordedGestures.Remove(lastGesture);
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
            e.GetComponent<EnergySegment>().SetColor(currColorPalette.GetNaryColor(0));
        }
        currSegments = segmentCount;

        if(spawnedMagicCircle == null){
            spawnedMagicCircle = Instantiate(magicCircle, magicCirclePosition);
        }
        ShowSpellEnergyUI();
    }
    void ClearGestureQueue(){
        foreach(GameObject g in recordedGestures){
            Destroy(g);
        }
        recordedGestures.Clear();
        UpdateGestureQueue();
        if(spawnedMagicCircle != null) {
            Destroy(spawnedMagicCircle);
            spawnedMagicCircle = null;
        }
        HideSpellEnergyUI();
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
            StartCoroutine(MoveGestureSymbol(gest, gest, gesturePositions[i], 0.5f));
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

    IEnumerator MoveGestureSymbol(GameObject g, GameObject start, GameObject target, float lerpTime){
        float timer = 0;
        while(timer < lerpTime){
            if (g == null) {
                break;
            }
            g.transform.position = Vector3.Lerp(start.transform.position, target.transform.position, timer/ lerpTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (g!=null) g.transform.position = target.transform.position;
    }
    IEnumerator ScaleTransitionSymbol(GameObject g, float startScale, float targetScale, float lerpTime){
        float timer = 0;
        while(timer < lerpTime){
            if (g == null) {
                break;
            }
            g.transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, timer/lerpTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (g!=null) {
            g.transform.localScale = Vector3.one * targetScale;
            if(targetScale == 0f){
                Destroy(g);
            }  
        }
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
            GameObject s = Instantiate(prefabToSpawn, position, Quaternion.identity);
            var main = s.GetComponent<ParticleSystem>().main;
            main.startColor = currColorPalette.GetNaryColor(0);
            foreach(ParticleSystem cp in s.transform.GetComponentsInChildren<ParticleSystem>()){
                var cp_main = cp.main;
                cp_main.startColor = currColorPalette.GetNaryColor(0);
            }
        }
    }

    void OnEnergyChange(int segmentsLeft, bool isBySwipe){
        if(currSegments - segmentsLeft > 1 || segmentsLeft < 0){
            return;
        }
        GameObject poofType;
        if(isBySwipe){
            poofType = poofEnergyChange;
        } else {
            poofType = poofTimerExpire;
        }
        for (int i = currSegments; i > segmentsLeft; i--){
            GameObject poof = Instantiate(poofType, energySegments[i-1].transform.position, Quaternion.identity);
            var main = poof.GetComponent<ParticleSystem>().main;
            main.startColor = ConvertColor(currColorPalette.GetNaryColor(0), main.startColor.color);
            spawnedEnergyBlocks[i-1].GetComponent<EnergySegment>().OnDeplete();
        }
        currSegments = segmentsLeft;
    }

    void OnTimeBarChange(float fill){
        energyTimerBar.GetComponent<SpellEnergyTimerBar>().SetFillAmount(fill);
    }

    void OnDestroy(){
        PlayerHealthControllable.OnHealthPercentChange -= UpdateHPBar;
        _deathSys.OnDeath -= OnDeath;
        PlayerDeathController.OnRespawn -= FadeInFromBlack;
        PlayerDeathController.OnRespawn -= ResetHPBar;
        PlayerCombatManager.OnEnterCombat -= ShowHUD;
        PlayerCombatManager.OnExitCombat -= HideHUD;        
        SpellSystem.GestureSequenceClearEvent -= ClearGestureQueue;
        SpellSystem.GestureSequenceAddEvent -= OnNewGesture;
        SpellSystem.SetSegmentFillEvent -= OnEnergyChange;
        SpellSystem.SetTimerBarPercentEvent -= OnTimeBarChange;
    }

    public Color ConvertColor(Color newColor, Color oldColor){
        Color.RGBToHSV(newColor, out var hue, out var s, out var v);
        Color.RGBToHSV(oldColor, out var h, out var sat, out var val);
        return Color.HSVToRGB(hue, sat, val);
    }

    void OnNewCapsule(int currentCharges, SpellElementColorPalette palette){
        if(palette.GetType() == typeof(FirePalette)){
            StartCoroutine(ShowNewEnergySegment(currentCharges, fireEnergySegment));
        } else if(palette.GetType() == typeof(IcePalette)){
            StartCoroutine(ShowNewEnergySegment(currentCharges, iceEnergySegment));
        } else if(palette.GetType() == typeof(WindPalette)){
            StartCoroutine(ShowNewEnergySegment(currentCharges, windEnergySegment));
        } else if(palette.GetType() == typeof(LightningPalette)){
            StartCoroutine(ShowNewEnergySegment(currentCharges, lightningEnergySegment));
        }
    }

    IEnumerator ShowNewEnergySegment(int currentCharges, GameObject segment){
        screenCenter.localScale = Vector3.one;
        float x = 0;
        List<GameObject> segments = new();
        
        //spawn all the existing segments as children of an object positioned in the center of the screen
        for(int i = 0; i < currentCharges; i++){
            GameObject g = Instantiate(segment, screenCenter);
            g.transform.position = screenCenter.transform.position + new Vector3(x, 0, 0);
            x -= segment.GetComponent<SpriteRenderer>().bounds.size.x;
            segments.Add(g);
        }
        GameObject spawnedCircle = Instantiate(gestureCircle, screenCenter);
        spawnedCircle.transform.position = screenCenter.position + new Vector3(segment.GetComponent<SpriteRenderer>().bounds.size.x, 0, 0);
        
        //fade them in
        StartCoroutine(TransitionChildrenUIColor(screenCenter, new Color(1,1,1,0), Color.white, 0.5f));
        StartCoroutine(ScaleObject(screenCenter, Vector3.one / 2, Vector3.one, 0.5f));

        //dont wait if there are no charges yet
        yield return new WaitForSeconds(1f);
        
        //move each segment to the left
        foreach(GameObject g in segments){
            StartCoroutine(MoveObject(g, g.transform.position - new Vector3(segment.GetComponent<SpriteRenderer>().bounds.size.x, 0, 0), 0.5f));
        }

        if(currentCharges > 0) yield return new WaitForSeconds(1f);

        //spawn the new segment
        GameObject newSegment = Instantiate(segment, screenCenter);
        newSegment.transform.position = screenCenter.transform.position + new Vector3(0,1,0);
        StartCoroutine(TransitionColor(newSegment.GetComponent<SpriteRenderer>(), new Color(1,1,1,0), Color.white, 0.5f));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MoveObjectCurved(newSegment, screenCenter.transform.position, 1f));
        
        yield return new WaitForSeconds(1f);

        //spawn particles
        Instantiate(newSegmentParticles, newSegment.transform.position, Quaternion.identity);
        
        yield return new WaitForSeconds(1f);
        StartCoroutine(TransitionChildrenUIColor(screenCenter, Color.white, new Color(1,1,1,0), 1f));
        StartCoroutine(ScaleObject(screenCenter, Vector3.one, Vector3.one / 2, 1f));
        yield return new WaitForSeconds(1f);
        foreach(Transform t in screenCenter){
            Destroy(t.gameObject);
        }
    }

    IEnumerator TransitionColor(SpriteRenderer s, Color startColor, Color endColor, float duration){
        float timer = 0;
        while (timer < duration){
            s.color = Color.Lerp(startColor, endColor, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        s.color = endColor;
    }


    //requires every child of the parent to have a sprite renderer
    IEnumerator TransitionChildrenUIColor(Transform parent, Color startColor, Color endColor, float duration){
        float timer = 0;
        while (timer < duration){
            foreach(SpriteRenderer s in parent.GetComponentsInChildren<SpriteRenderer>()){
                s.color = Color.Lerp(startColor, endColor, timer / duration);
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach(SpriteRenderer s in parent.GetComponentsInChildren<SpriteRenderer>()){
            s.color = endColor;
        }
    }

    IEnumerator ScaleObject(Transform g, Vector3 startScale, Vector3 targetScale, float duration){
        float timer = 0;
        while(timer < duration){
            g.localScale = Vector3.Lerp(startScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        g.localScale = targetScale;
    }

    IEnumerator MoveObject(GameObject g, Vector3 target, float duration){
        float timer = 0;
        Vector3 startPos = g.transform.position;
        while(timer < duration){
            g.transform.position = Vector3.Lerp(startPos, target, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        g.transform.position = target;
    }
    IEnumerator MoveObjectCurved(GameObject g, Vector3 target, float duration){
        float timer = 0;
        Vector3 startPos = g.transform.position;
        while(timer < duration){
            g.transform.position = Vector3.Lerp(startPos, target, movementCurve.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        g.transform.position = target;
    }
}
