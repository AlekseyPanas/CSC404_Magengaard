using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : NetworkBehaviour {
    public static event Action<string> OnSceneChanged = delegate {};

    public string nextSceneToLoad;
    public FadeToBlackPanel panel;

    public void SwitchScene() {
        OnSceneChanged(nextSceneToLoad);
        NetworkManager.SceneManager.LoadScene(nextSceneToLoad, LoadSceneMode.Single);
    }
    
    public void Transition()
    {
        panel.gameObject.SetActive(true);
        panel.startFadingToBlack(1.0f);
        
        Invoke(nameof(SwitchScene), 1.0f);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) Transition();
    }
}
