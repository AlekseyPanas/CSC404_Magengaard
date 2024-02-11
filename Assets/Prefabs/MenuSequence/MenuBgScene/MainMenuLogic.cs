using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private string scenePath;
    
    private bool clicked = false;
    private DesktopControls _controls;

    void Start () {
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        InputSystem.onAnyButtonPress.CallOnce(_ => { 
            clicked = true; 

        });
    }

    void Update () {
        
    }

    private IEnumerator fadeToBlack() {
        
        yield return new WaitForSeconds(0.005f);
    }

    public async void Host()
    {
        await SceneManager.LoadSceneAsync(scenePath);

        networkManager.StartHost();
    }

    public void Quit() {
        Application.Quit();
    }
}
