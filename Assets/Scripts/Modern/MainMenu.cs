using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public NetworkManager networkManager;
    public string scenePath;

    public TMP_InputField addressField;
    
    public async void Host()
    {
        await SceneManager.LoadSceneAsync(scenePath);

        networkManager.StartHost();
    }

    public async void Connect()
    {
        var address = addressField.text;
        var transport = networkManager.GetComponent<UnityTransport>();
            
        transport.SetConnectionData(address, 7777);
        
        await SceneManager.LoadSceneAsync(scenePath);
        
        networkManager.StartClient();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
