using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Applies saved avatar customization from AvatarDataStore on the local player.
/// Only the owner applies local data; remote players get their look from NetworkAvatarState.
/// Attach to the Player prefab alongside NetworkObject.
/// </summary>
public class NetworkAvatarApplier : NetworkBehaviour
{
    [Tooltip("Avatar root transform (the 'Idle' object). Auto-found if null.")]
    public Transform avatarRoot;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the local player applies saved customization from AvatarDataStore
        if (!IsOwner) return;

        // Find avatar root if not assigned
        if (avatarRoot == null)
        {
            avatarRoot = transform.Find("Idle");
            if (avatarRoot == null)
            {
                // Try finding any child starting with "Idle"
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("Idle"))
                    {
                        avatarRoot = child;
                        break;
                    }
                }
            }
            if (avatarRoot == null)
            {
                // Fallback: child with Animator
                Animator anim = GetComponentInChildren<Animator>();
                if (anim != null) avatarRoot = anim.transform;
            }
        }

        if (avatarRoot != null && AvatarDataStore.HasData)
        {
            AvatarDataStore.ApplyToAvatar(avatarRoot);
            Debug.Log("[NetworkAvatarApplier] Local avatar customization applied for owner");
        }
        else if (!AvatarDataStore.HasData)
        {
            Debug.LogWarning("[NetworkAvatarApplier] No avatar data saved — using default appearance");
        }
        else
        {
            Debug.LogWarning("[NetworkAvatarApplier] Could not find avatar root to apply data");
        }
    }
}
