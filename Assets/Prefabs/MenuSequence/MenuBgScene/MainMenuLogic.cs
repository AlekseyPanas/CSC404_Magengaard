using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public delegate void KeyPressed();

public class MainMenuLogic : AWaitFor
{
    public static MainMenuLogic instance;  // Singleton

    // Tracks registered objects. Will not fade into the cutscene section until all registerees fire their FinishedTaskEvent
    private int registeredWaitFor = 0;
    private int respondedWaitFor = 0; 
    public void RegisterWaitFor(AWaitFor w) { 
        registeredWaitFor++; 
        w.FinishedTaskEvent += () => { 
            
            respondedWaitFor++; 
            
            if (registeredWaitFor == respondedWaitFor) {
                StartCoroutine(fadeToBlack());
            }
        }; 
    }

    // Subscribe to this static event from another script to know when the player has "pressed any key to continue"
    public static event KeyPressed KeyPressedEvent = delegate {};  // Called when a key is pressed and the game will continue

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private string scenePath;
    [SerializeField] private Image fadeToBlackPanelImage;
    
    private DesktopControls _controls;

    // Set singleton stuff
    void Awake() {
        if (instance != null && instance != this) { Destroy(gameObject); }
        else { instance = this; }
    }

    // Initialize controls and fade into the scene
    void Start () {
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
        StartCoroutine(fadeToTransparent());
    }

    // Fade out into black and switch to cutscene afterwards
    private IEnumerator fadeToBlack() {
        while (fadeToBlackPanelImage.color.a < 1) {
            Color col = fadeToBlackPanelImage.color;
            fadeToBlackPanelImage.color = new Color(col.r, col.g, col.b, col.a + 0.01f);
            yield return new WaitForSeconds(0.01f);    
        }
        yield return new WaitForSeconds(1f);
        invokeFinishedTask();
    }

    // Co-routine to fade from black into alpha
    private IEnumerator fadeToTransparent() {
        while (fadeToBlackPanelImage.color.a > 0) {
            Color col = fadeToBlackPanelImage.color;
            fadeToBlackPanelImage.color = new Color(col.r, col.g, col.b, col.a - 0.01f);
            yield return new WaitForSeconds(0.01f);    
        }
        InputSystem.onAnyButtonPress.CallOnce(_ => { KeyPressedEvent(); });
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
