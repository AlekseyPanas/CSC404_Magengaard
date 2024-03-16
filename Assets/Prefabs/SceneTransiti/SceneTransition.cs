using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string destination;
    public FadeToBlackPanel panel;

    public void SwitchScene()
    {
        SceneManager.LoadScene(destination);
    }
    
    public void Transition()
    {
        panel.gameObject.SetActive(true);
        panel.startFadingToBlack(1.0f);
        
        Invoke(nameof(SwitchScene), 1.0f);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Transition();
    }
}
