using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvas : NetworkBehaviour
{
    [SerializeField] Button hostBtn;
    [SerializeField] Button serverBtn;
    [SerializeField] Button clientBtn;
    void Awake (){
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
}
