using UnityEngine;

/// <summary>
/// Attach this script to the root "Idle" object of your Avatar in the DashboardAvatarScene.
/// It will automatically apply the customization saved in AvatarDataStore.
/// </summary>
public class DashboardAvatarManager : MonoBehaviour
{
    void Start()
    {
        // When the Dashboard Showcase scene loads, immediately apply the customized avatar state!
        if (AvatarDataStore.HasData)
        {
            AvatarDataStore.ApplyToAvatar(this.transform);
            Debug.Log("[DashboardAvatarManager] Successfully applied customized avatar state.");
        }
        else
        {
            Debug.LogWarning("[DashboardAvatarManager] No avatar data found in AvatarDataStore.");
        }
    }
}
