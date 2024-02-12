using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireSceneCamera : MonoBehaviour, IWaitBeforeFade
{

    // Stores orientation references to use when moving the camera around
    private Quaternion baseRot;
    private Vector3 basePos;

    // Implements IWaitBeforeFade interace
    public event FinishedTask FinishedTaskEvent = delegate {};
    
    // Flag set when MainMenuLogic sends the event
    private bool isKeyPressed = false;

    // Start is called before the first frame update
    void Start() {
        // Record orientation data
        baseRot = transform.rotation;
        basePos = transform.position;

        MainMenuLogic.instance.RegisterWaitFor(this);

        MainMenuLogic.KeyPressedEvent += () => {
            isKeyPressed = true;
            StartCoroutine(layDown());
        };
    }

    // Update is called once per frame
    void Update() {
        if (!isKeyPressed) {
            transform.rotation = new Quaternion(baseRot.x + headTurnPeriodicFunction(0.1f, 0.0001f, 0.4f, 4.7f, Time.time), baseRot.y + headTurnPeriodicFunction(0.1f, 0.01f, 3.5f, 0.1f, Time.time), baseRot.z, baseRot.w);
            transform.position = new Vector3(basePos.x, basePos.y, basePos.z);
        }
    }

    /** A Desmos-derived sin wave combination which simulates realistic-ish head-turning rotations */
    float headTurnPeriodicFunction(float a, float b, float c, float d, float x) {
        return _headTurnInnerFunction(a, b, d, c * x) + _headTurnInnerFunction(a, b, d, (-c * x) + 5);
    }

    float _headTurnInnerFunction(float a, float b, float d, float x) {
        return b * Mathf.Pow(Mathf.Sin(a * x) - Mathf.Sin(a / 2f * x) - Mathf.Sin(2f * a * x) + d * Mathf.Sin(x), 3);
    }

    // Moves and rotates camera to make it as if the player is going to sleep (first person)
    IEnumerator layDown() {
        float yInc = (basePos.y - 0.3f) / 90;  // To move camera down 0.3f above the ground

        // Rotate and move down over time
        for (int i = 0; i < 90; i++) {
            transform.Rotate(new Vector3(0f, 0f, 1f));
            transform.position = new Vector3(transform.position.x - 0.02f, transform.position.y - yInc, transform.position.z);

            yield return new WaitForSeconds(0.02f);
        }

        // Notify MainMenuLogic that it is alright to move on
        FinishedTaskEvent();
    }
}
