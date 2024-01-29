//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Actions/DesktopControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DesktopControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DesktopControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DesktopControls"",
    ""maps"": [
        {
            ""name"": ""Game"",
            ""id"": ""874a0c11-dbb5-4cc0-81df-2e96007d8628"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""766e30d3-9f87-46c5-9d31-2026a3895daa"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""4ef05bb5-788e-4db1-bc15-b132621f1eaf"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""0af9619d-3d81-44ac-b80d-d98d15e8948d"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""748c5195-80bc-48c2-8e2a-8ba17ab9961d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""10d30cc2-592a-4e2d-b10a-70ed6618edfc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""0250fa89-fd7d-4b46-87f5-2eff511595a3"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""16cc4fea-8dd2-424b-b314-ebedd55c9349"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Development"",
            ""id"": ""3092c2ee-2010-47dc-8f8f-136aa6eb5dd7"",
            ""actions"": [
                {
                    ""name"": ""ConnectLocally"",
                    ""type"": ""Button"",
                    ""id"": ""b462c5d7-aeb3-4d8a-b24c-6451b6ea96d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""HostLocally"",
                    ""type"": ""Button"",
                    ""id"": ""046793ae-2fda-4ae6-ba18-99ff1d582803"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2c6371e7-f8cc-404f-9178-ac598f5e0bf4"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ConnectLocally"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""545ce344-9ba0-4b94-821f-f88b8577a7ae"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HostLocally"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Game
        m_Game = asset.FindActionMap("Game", throwIfNotFound: true);
        m_Game_Movement = m_Game.FindAction("Movement", throwIfNotFound: true);
        // Development
        m_Development = asset.FindActionMap("Development", throwIfNotFound: true);
        m_Development_ConnectLocally = m_Development.FindAction("ConnectLocally", throwIfNotFound: true);
        m_Development_HostLocally = m_Development.FindAction("HostLocally", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Game
    private readonly InputActionMap m_Game;
    private List<IGameActions> m_GameActionsCallbackInterfaces = new List<IGameActions>();
    private readonly InputAction m_Game_Movement;
    public struct GameActions
    {
        private @DesktopControls m_Wrapper;
        public GameActions(@DesktopControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Game_Movement;
        public InputActionMap Get() { return m_Wrapper.m_Game; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameActions set) { return set.Get(); }
        public void AddCallbacks(IGameActions instance)
        {
            if (instance == null || m_Wrapper.m_GameActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_GameActionsCallbackInterfaces.Add(instance);
            @Movement.started += instance.OnMovement;
            @Movement.performed += instance.OnMovement;
            @Movement.canceled += instance.OnMovement;
        }

        private void UnregisterCallbacks(IGameActions instance)
        {
            @Movement.started -= instance.OnMovement;
            @Movement.performed -= instance.OnMovement;
            @Movement.canceled -= instance.OnMovement;
        }

        public void RemoveCallbacks(IGameActions instance)
        {
            if (m_Wrapper.m_GameActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IGameActions instance)
        {
            foreach (var item in m_Wrapper.m_GameActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_GameActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public GameActions @Game => new GameActions(this);

    // Development
    private readonly InputActionMap m_Development;
    private List<IDevelopmentActions> m_DevelopmentActionsCallbackInterfaces = new List<IDevelopmentActions>();
    private readonly InputAction m_Development_ConnectLocally;
    private readonly InputAction m_Development_HostLocally;
    public struct DevelopmentActions
    {
        private @DesktopControls m_Wrapper;
        public DevelopmentActions(@DesktopControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ConnectLocally => m_Wrapper.m_Development_ConnectLocally;
        public InputAction @HostLocally => m_Wrapper.m_Development_HostLocally;
        public InputActionMap Get() { return m_Wrapper.m_Development; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DevelopmentActions set) { return set.Get(); }
        public void AddCallbacks(IDevelopmentActions instance)
        {
            if (instance == null || m_Wrapper.m_DevelopmentActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_DevelopmentActionsCallbackInterfaces.Add(instance);
            @ConnectLocally.started += instance.OnConnectLocally;
            @ConnectLocally.performed += instance.OnConnectLocally;
            @ConnectLocally.canceled += instance.OnConnectLocally;
            @HostLocally.started += instance.OnHostLocally;
            @HostLocally.performed += instance.OnHostLocally;
            @HostLocally.canceled += instance.OnHostLocally;
        }

        private void UnregisterCallbacks(IDevelopmentActions instance)
        {
            @ConnectLocally.started -= instance.OnConnectLocally;
            @ConnectLocally.performed -= instance.OnConnectLocally;
            @ConnectLocally.canceled -= instance.OnConnectLocally;
            @HostLocally.started -= instance.OnHostLocally;
            @HostLocally.performed -= instance.OnHostLocally;
            @HostLocally.canceled -= instance.OnHostLocally;
        }

        public void RemoveCallbacks(IDevelopmentActions instance)
        {
            if (m_Wrapper.m_DevelopmentActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IDevelopmentActions instance)
        {
            foreach (var item in m_Wrapper.m_DevelopmentActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_DevelopmentActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public DevelopmentActions @Development => new DevelopmentActions(this);
    public interface IGameActions
    {
        void OnMovement(InputAction.CallbackContext context);
    }
    public interface IDevelopmentActions
    {
        void OnConnectLocally(InputAction.CallbackContext context);
        void OnHostLocally(InputAction.CallbackContext context);
    }
}
