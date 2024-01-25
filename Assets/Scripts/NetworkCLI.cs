using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCLI : MonoBehaviour
{
    private NetworkManager _manager;
    
    void Start()
    {
        _manager = GetComponentInParent<NetworkManager>();

        // if (Application.isEditor)
        // {
        //     return;
        // }

        var args = Environment.GetCommandLineArgs();
        var mode = "host";

        var serverIndex = args.ToList().IndexOf("-server");

        if (serverIndex >= 0)
        {
            mode = args[serverIndex + 1]; // For sanity's sake.
        }
        
        Console.WriteLine("Starting client with server mode {0}", mode);
        
        switch (mode)
        {
            case "server":
                Console.WriteLine("Starting server");
                _manager.StartServer();
                break;
            
            case "host":
                Console.WriteLine("Starting host");
                _manager.StartHost();
                break;
            
            case "client":
                Console.WriteLine("Starting client");
                _manager.StartClient();
                break;
            
            default:
                throw new Exception("Invalid networking mode " + mode);
        }
    }
}
