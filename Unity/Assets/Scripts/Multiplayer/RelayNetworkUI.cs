using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

/// <summary>
/// UI controller for Relay-based multiplayer lobby.
/// Shows Create/Join buttons, join code display/input, connection status, and disconnect.
/// Attach to a Canvas in the SpawnArea scene.
/// </summary>
public class RelayNetworkUI : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel shown before connecting (create/join buttons)")]
    public GameObject lobbyPanel;
    [Tooltip("Panel shown after connecting (join code display, disconnect)")]
    public GameObject connectedPanel;

    [Header("Lobby UI")]
    public Button createRoomButton;
    public TMP_InputField joinCodeInput;
    public Button joinRoomButton;
    public TextMeshProUGUI statusText;

    [Header("Connected UI")]
    public TextMeshProUGUI joinCodeDisplay;
    public TextMeshProUGUI playerCountText;
    public Button disconnectButton;

    void Start()
    {
        // Initially show lobby, hide connected panel
        ShowLobbyPanel();

        createRoomButton.onClick.AddListener(OnCreateRoom);
        joinRoomButton.onClick.AddListener(OnJoinRoom);
        disconnectButton.onClick.AddListener(OnDisconnect);

        // Listen for relay events
        if (RelayManager.Instance != null)
        {
            RelayManager.Instance.OnJoinCodeGenerated += OnJoinCodeReceived;
            RelayManager.Instance.OnError += OnRelayError;
            RelayManager.Instance.OnConnected += OnConnectionEstablished;
        }
    }

    void OnDestroy()
    {
        if (RelayManager.Instance != null)
        {
            RelayManager.Instance.OnJoinCodeGenerated -= OnJoinCodeReceived;
            RelayManager.Instance.OnError -= OnRelayError;
            RelayManager.Instance.OnConnected -= OnConnectionEstablished;
        }
    }

    void Update()
    {
        // Update player count when connected
        if (connectedPanel.activeSelf && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            int count = NetworkManager.Singleton.ConnectedClientsList.Count;
            playerCountText.text = $"Players: {count}";
        }
    }

    async void OnCreateRoom()
    {
        SetStatus("Creating room...");
        SetButtonsInteractable(false);

        string code = await RelayManager.Instance.CreateRelay();
        if (code == null)
        {
            SetStatus("Failed to create room. Try again.");
            SetButtonsInteractable(true);
        }
    }

    async void OnJoinRoom()
    {
        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Enter a join code");
            return;
        }

        SetStatus($"Joining room: {code}...");
        SetButtonsInteractable(false);

        bool success = await RelayManager.Instance.JoinRelay(code);
        if (!success)
        {
            SetStatus("Failed to join. Check the code and try again.");
            SetButtonsInteractable(true);
        }
    }

    void OnDisconnect()
    {
        RelayManager.Instance.Disconnect();
        ShowLobbyPanel();
        SetStatus("Disconnected");
    }

    void OnJoinCodeReceived(string code)
    {
        joinCodeDisplay.text = $"Join Code: {code}";
    }

    void OnConnectionEstablished()
    {
        ShowConnectedPanel();
    }

    void OnRelayError(string error)
    {
        SetStatus(error);
        SetButtonsInteractable(true);
    }

    void ShowLobbyPanel()
    {
        lobbyPanel.SetActive(true);
        connectedPanel.SetActive(false);
        SetButtonsInteractable(true);
    }

    void ShowConnectedPanel()
    {
        lobbyPanel.SetActive(false);
        connectedPanel.SetActive(true);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log($"[RelayNetworkUI] {message}");
    }

    void SetButtonsInteractable(bool interactable)
    {
        createRoomButton.interactable = interactable;
        joinRoomButton.interactable = interactable;
    }
}
