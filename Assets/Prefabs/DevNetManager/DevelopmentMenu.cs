using System;
using Unity.Netcode;
using UnityEngine;

namespace Modern
{
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
            
        }

        public void Host()
        {
            manager.StartHost();
            
            gameObject.SetActive(false);
            developmentObject.SetActive(false);
        }

        public void Connect()
        {
            manager.StartClient();
            
            gameObject.SetActive(false);
            developmentObject.SetActive(false);
        }
    }
}