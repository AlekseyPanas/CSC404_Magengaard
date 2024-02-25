using System;
using Unity.Netcode;
using UnityEngine;

// Menu that appears in the DevelopmentNetworkManager prefab.
public class DevelopmentMenu: MonoBehaviour
{
    public NetworkManager manager;
    
    // Disabled on connection.
    public GameObject developmentObject;

    private DesktopControls _controls;

    private void Start()
    {
        _controls = new DesktopControls();
        
        _controls.Enable();
        _controls.Development.Enable();

        _controls.Development.HostLocally.performed += context => { Host(); };
        _controls.Development.ConnectLocally.performed += context => { Connect(); };
        Time.timeScale = 0;
    }

    public void Host()
    {
        Time.timeScale = 1;
        manager.StartHost();
        
        gameObject.SetActive(false);
        developmentObject.SetActive(false);
    }

    public void Connect()
    {
        Time.timeScale = 1;
        manager.StartClient();
        
        gameObject.SetActive(false);
        developmentObject.SetActive(false);
    }
}
