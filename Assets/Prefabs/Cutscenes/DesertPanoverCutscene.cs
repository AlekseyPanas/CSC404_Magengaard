using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertPanoverCutscene : Cutscene
{
    public Transform panoverStart;
    public Transform panoverEnd;

    public Animator canvas;
    private static readonly int Fade = Animator.StringToHash("Fade");

    private IEnumerator CutsceneEvents()
    {
        yield return 0; // allow time for the rest of the game to get working
        
        var player = GameObject.FindWithTag("Player");
        
        LoadWithPlayer(player.transform);

        if (!TryRegister())
        {
            yield break;
        }
        
        var cameraManager = _cameraSystem.GetSystem(_cameraSystemRegistrant);

        if (cameraManager == null)
        {
            yield break;
        }

        var initialFollow = cameraManager.GetCurrentFollow(_cameraSystemRegistrant);

        canvas.gameObject.SetActive(true);
        
        cameraManager.JumpTo(ToPosition(panoverStart));
        cameraManager.SwitchFollow(_cameraSystemRegistrant,
            new CameraFollowFixed(panoverEnd.position, panoverEnd.forward, 5.0f));
        
        yield return new WaitForSeconds(3.0f);
        
        cameraManager.SwitchFollow(_cameraSystemRegistrant, initialFollow);
        
        canvas.SetBool(Fade, true);
        
        // End
        
        DeRegisterAll();
    }
    
    public void StartCutscene()
    {
        StartCoroutine(CutsceneEvents());
    }

    private void Start()
    {
        StartCutscene();
    }
}
