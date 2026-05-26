using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static data store that holds avatar customization state between scenes.
/// Stores which child object is active under each category (hair, clothes, beard, accessories, shoes).
/// </summary>
public static class AvatarDataStore
{
    // Maps category name (e.g. "hair") to the name of the active child GameObject
    private static Dictionary<string, string> activeChildren = new Dictionary<string, string>();

    // Skin material index from SkinColorChanger
    public static int SkinIndex = -1;

    // Whether data has been saved (so we don't apply empty data)
    public static bool HasData = false;

    // The category names that we look for under the Idle object
    private static readonly string[] categories = { "hair", "clothes", "beard", "accesories", "shoes" };

    /// <summary>
    /// Saves the current avatar state from the Idle object's hierarchy.
    /// Call this before loading the next scene.
    /// </summary>
    /// <param name="idleTransform">The root "Idle" transform containing category children.</param>
    public static void SaveFromAvatar(Transform idleTransform)
    {
        activeChildren.Clear();

        foreach (string category in categories)
        {
            Transform categoryTransform = idleTransform.Find(category);
            if (categoryTransform == null)
            {
                Debug.LogWarning($"[AvatarDataStore] Category '{category}' not found under '{idleTransform.name}'");
                continue;
            }

            // Find which child is active in this category
            for (int i = 0; i < categoryTransform.childCount; i++)
            {
                Transform child = categoryTransform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    activeChildren[category] = child.name;
                    Debug.Log($"[AvatarDataStore] Saved: {category} → {child.name}");
                    break;
                }
            }
        }

        // Also save the direct children of Idle that are not categories
        // (e.g. Human.005, Human.high-poly.002, Human.mindfront_eyelashes_01.002)
        for (int i = 0; i < idleTransform.childCount; i++)
        {
            Transform child = idleTransform.GetChild(i);
            // Skip category folders — we already handled those
            bool isCategory = false;
            foreach (string cat in categories)
            {
                if (child.name.ToLower() == cat.ToLower())
                {
                    isCategory = true;
                    break;
                }
            }
            if (!isCategory)
            {
                activeChildren["_direct_" + child.name] = child.gameObject.activeSelf ? "1" : "0";
            }
        }

        // Save skin index from SkinColorChanger if present
        SkinColorChanger skinChanger = idleTransform.GetComponentInChildren<SkinColorChanger>();
        if (skinChanger != null)
        {
            SkinIndex = skinChanger.GetCurrentSkinIndex();
            Debug.Log($"[AvatarDataStore] Saved skin index: {SkinIndex}");
        }

        HasData = true;
        Debug.Log($"[AvatarDataStore] Avatar state saved. {activeChildren.Count} entries.");
    }

    /// <summary>
    /// Applies the saved avatar state to an Idle object in the new scene.
    /// </summary>
    /// <param name="idleTransform">The root "Idle" transform to apply state to.</param>
    public static void ApplyToAvatar(Transform idleTransform)
    {
        if (!HasData)
        {
            Debug.LogWarning("[AvatarDataStore] No saved data to apply.");
            return;
        }

        // Apply category children (activate the correct one, deactivate others)
        foreach (string category in categories)
        {
            Transform categoryTransform = idleTransform.Find(category);
            if (categoryTransform == null)
            {
                Debug.LogWarning($"[AvatarDataStore] Category '{category}' not found under '{idleTransform.name}'");
                continue;
            }

            string activeChildName;
            if (activeChildren.TryGetValue(category, out activeChildName))
            {
                for (int i = 0; i < categoryTransform.childCount; i++)
                {
                    Transform child = categoryTransform.GetChild(i);
                    bool shouldBeActive = (child.name == activeChildName);
                    child.gameObject.SetActive(shouldBeActive);
                }
                Debug.Log($"[AvatarDataStore] Applied: {category} → {activeChildName}");
            }
        }

        // Apply direct children active states
        for (int i = 0; i < idleTransform.childCount; i++)
        {
            Transform child = idleTransform.GetChild(i);
            string key = "_direct_" + child.name;
            string value;
            if (activeChildren.TryGetValue(key, out value))
            {
                child.gameObject.SetActive(value == "1");
            }
        }

        // Apply skin material
        if (SkinIndex >= 0)
        {
            SkinColorChanger skinChanger = idleTransform.GetComponentInChildren<SkinColorChanger>();
            if (skinChanger != null)
            {
                skinChanger.SelectSkin(SkinIndex);
                Debug.Log($"[AvatarDataStore] Applied skin index: {SkinIndex}");
            }
        }

        Debug.Log("[AvatarDataStore] Avatar state applied.");
    }

    [System.Serializable]
    public class AvatarSaveData
    {
        public int SkinIndex;
        public string[] Keys;
        public string[] Values;
    }

    /// <summary>
    /// Serializes current state to JSON string for React bridge
    /// </summary>
    public static string ToJson()
    {
        AvatarSaveData data = new AvatarSaveData();
        data.SkinIndex = SkinIndex;
        
        int count = activeChildren.Count;
        data.Keys = new string[count];
        data.Values = new string[count];
        
        int i = 0;
        foreach (var kvp in activeChildren)
        {
            data.Keys[i] = kvp.Key;
            data.Values[i] = kvp.Value;
            i++;
        }
        
        return JsonUtility.ToJson(data);
    }

    /// <summary>
    /// Deserializes state from JSON string passed from React bridge
    /// </summary>
    public static void FromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        
        AvatarSaveData data = JsonUtility.FromJson<AvatarSaveData>(json);
        if (data != null)
        {
            SkinIndex = data.SkinIndex;
            activeChildren.Clear();
            if (data.Keys != null && data.Values != null && data.Keys.Length == data.Values.Length)
            {
                for (int i = 0; i < data.Keys.Length; i++)
                {
                    activeChildren[data.Keys[i]] = data.Values[i];
                }
            }
            HasData = true;
            Debug.Log($"[AvatarDataStore] Hydrated from JSON: {activeChildren.Count} entries");
        }
    }
}
