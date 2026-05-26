using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex = 1;

    [Header("Avatar Root (the 'Idle' object)")]
    [Tooltip("Drag the Idle transform here so we can save its state before loading.")]
    public Transform avatarRoot;

    [DllImport("__Internal")]
    private static extern void NotifyAvatarComplete(string jsonStr);

    /// <summary>
    /// Saves the avatar customization state, then loads the next scene.
    /// Hook the "Complete" button's OnClick to this method.
    /// </summary>
    public void SaveAndLoadScene()
    {
        if (avatarRoot != null)
        {
            AvatarDataStore.SaveFromAvatar(avatarRoot);
            Debug.Log("[SceneLoader] Avatar state saved. Loading scene index: " + sceneIndex);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] avatarRoot is not assigned! Avatar state will NOT be saved.");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // Send state to React and let React handle scene loading via unmount/remount
        string json = AvatarDataStore.ToJson();
        NotifyAvatarComplete(json);
#else
        SceneManager.LoadScene(sceneIndex);
#endif
    }

    /// <summary>
    /// Loads the scene without saving avatar state (original behavior).
    /// </summary>
    public void LoadSceneByIndex()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Called by React to transition into the Dashboard showcase state
    /// </summary>
    public void LoadDashboardScene()
    {
        SceneManager.LoadScene("DashboardAvatarScene");
    }
}