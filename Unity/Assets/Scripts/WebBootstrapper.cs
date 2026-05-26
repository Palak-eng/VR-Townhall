using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this script to a GameObject named "WebBootstrapper" in your initial Unity scene.
/// This acts as the entry point from React when remounting Unity for the multiplayer sequence.
/// </summary>
public class WebBootstrapper : MonoBehaviour
{
    private static WebBootstrapper instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called by React via SendMessage("WebBootstrapper", "StartMultiplayer", avatarJson)
    /// This method will parse the avatarJson and immediately load the multiplayer scene.
    /// </summary>
    /// <param name="avatarJson">The JSON string containing the avatar state</param>
    public void StartMultiplayer(string avatarJson)
    {
        Debug.Log($"[WebBootstrapper] Received StartMultiplayer from React. JSON length: {avatarJson?.Length}");
        
        // Hydrate the shared AvatarDataStore so the multiplayer scene can apply it
        if (!string.IsNullOrEmpty(avatarJson))
        {
            AvatarDataStore.FromJson(avatarJson);
        }
        else 
        {
            Debug.LogWarning("[WebBootstrapper] Received empty avatarJson. Skipping hydration.");
        }

        // Load the Multiplayer Lobby/Spawn scene (adjust build index 1 as needed)
        SceneManager.LoadScene(1); 
    }
}
