using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// Manages Unity Relay connections. Handles service initialization,
/// relay allocation (host), and relay joining (client).
/// Attach to a persistent GameObject in the SpawnArea scene.
/// </summary>
public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [Header("Relay Settings")]
    [Tooltip("Max players that can join a relay session (excluding host)")]
    public int maxConnections = 50;

    // Current join code after hosting
    public string JoinCode { get; private set; }

    // Whether Unity Services have been initialized
    public bool IsInitialized { get; private set; }

    // Events for UI to listen to
    public event Action<string> OnJoinCodeGenerated;
    public event Action<string> OnError;
    public event Action OnConnected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    async void Start()
    {
        await InitializeServices();
    }

    /// <summary>
    /// Initializes Unity Services and signs in anonymously.
    /// </summary>
    async Task InitializeServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[RelayManager] Signed in. Player ID: {AuthenticationService.Instance.PlayerId}");
            }

            IsInitialized = true;
            Debug.Log("[RelayManager] Unity Services initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayManager] Failed to initialize services: {e.Message}");
            OnError?.Invoke($"Service init failed: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a Relay allocation, configures transport, starts host, and returns the join code.
    /// </summary>
    public async Task<string> CreateRelay()
    {
        if (!IsInitialized)
        {
            await InitializeServices();
        }

        try
        {
            // Allocate a relay server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"[RelayManager] Relay created. Join code: {JoinCode}");

            // Configure the Unity Transport with relay server data
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Start hosting
            NetworkManager.Singleton.StartHost();
            Debug.Log("[RelayManager] Host started via Relay");

            OnJoinCodeGenerated?.Invoke(JoinCode);
            OnConnected?.Invoke();

            return JoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayManager] Relay creation failed: {e.Message}");
            OnError?.Invoke($"Relay failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Joins an existing Relay session using a join code, configures transport, and starts client.
    /// </summary>
    public async Task<bool> JoinRelay(string joinCode)
    {
        if (!IsInitialized)
        {
            await InitializeServices();
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log($"[RelayManager] Joined relay with code: {joinCode}");

            // Configure the Unity Transport with relay join data
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Start as client
            NetworkManager.Singleton.StartClient();
            Debug.Log("[RelayManager] Client started via Relay");

            OnConnected?.Invoke();

            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayManager] Relay join failed: {e.Message}");
            OnError?.Invoke($"Join failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Disconnects from the current session.
    /// </summary>
    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[RelayManager] Disconnected from relay session");
        }
    }
}
