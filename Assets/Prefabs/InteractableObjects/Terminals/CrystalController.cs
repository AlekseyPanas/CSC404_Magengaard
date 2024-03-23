using System.Collections;
using UnityEngine;

public class CrystalController : MonoBehaviour
{
    GameObject followTarget = null;
    [SerializeField] float bobHeight;
    [SerializeField] float circleRadius;
    [SerializeField] float circleSpeed;
    [SerializeField] GameObject targetTerminal;
    [SerializeField] Transform terminalCrystalPosition;
    [SerializeField] float moveToTargetDuration;
    [SerializeField] float moveToTerminalDuration;
    [SerializeField] float passiveRotateSpeed;
    [SerializeField] ParticleSystem ps;
    Vector3 startPos;
    Vector3 offsetFromTarget;
    bool moveStarted = false;
    bool startedCircling = false;
    bool foundTerminal = false;

    void OnTriggerEnter(Collider col){
        if(col.CompareTag("Player")){
            followTarget = col.gameObject;
            if(!moveStarted) {
                StartCoroutine(MoveToTarget());
                moveStarted = true;
            }
        }
        else if(col.gameObject == targetTerminal) {
            if(!foundTerminal) {
                StartCoroutine(MoveToTerminal());
                foundTerminal = true;
            } 
        }
    }

    void Bob(){
        float y = Mathf.Sin(Time.time) * bobHeight;
        transform.position = startPos + new Vector3(0, y, 0);
    }

    void CircleTarget(){
        offsetFromTarget = Quaternion.Euler(0, circleSpeed, 0) * offsetFromTarget;
        startPos = followTarget.transform.position + offsetFromTarget;
        Bob();
    }


    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if(!startedCircling) transform.Rotate(0, passiveRotateSpeed * Time.deltaTime, 0);
        if(followTarget == null){
            Bob();
        } else {
            if(startedCircling && !foundTerminal){
                CircleTarget();
            }
        }
    }

    IEnumerator MoveToTarget(){
        float timer = 0;
        Vector3 origin = transform.position;
        Vector3 target;
        Vector3 diff = Vector3.zero;
        Vector3 diffXZ = Vector3.zero;
        while (timer < moveToTargetDuration){
            timer += Time.deltaTime;
            diff = followTarget.transform.position - transform.position;
            diffXZ = new(diff.x, 0, diff.z);
            target = followTarget.transform.position + (diffXZ.normalized * circleRadius);
            transform.position = Vector3.Lerp(origin, target, timer / moveToTargetDuration);
            if(diffXZ.magnitude < circleRadius) break;
            yield return new WaitForEndOfFrame();
        }
        offsetFromTarget = -diffXZ;
        startPos = transform.position;
        startedCircling = true;
    }

    IEnumerator MoveToTerminal(){
        float timer = 0;
        Vector3 startPos = transform.position;
        while (timer < moveToTerminalDuration){
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, terminalCrystalPosition.position, timer/moveToTerminalDuration);
            yield return new WaitForEndOfFrame();
        }
        transform.position = terminalCrystalPosition.position;
        targetTerminal.GetComponent<ITerminal>().PlaceCrystal(gameObject);
        //Destroy(gameObject);
    }

    public void StopPS(){
        ps.Stop();
    }
}
