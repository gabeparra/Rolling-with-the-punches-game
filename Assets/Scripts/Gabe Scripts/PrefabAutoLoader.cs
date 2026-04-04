using UnityEngine;

public static class PrefabAutoLoader
{
    public static GameObject[] EnsureLoaded(GameObject[] inspectorArray, string resourcePath)
    {
        if (inspectorArray != null && inspectorArray.Length > 0)
        {
            bool allValid = true;
            foreach (var go in inspectorArray)
            {
                if (go == null) { allValid = false; break; }
            }
            if (allValid) return inspectorArray;
        }

        GameObject[] loaded = Resources.LoadAll<GameObject>(resourcePath);
        if (loaded.Length > 0)
            Debug.Log($"[PrefabAutoLoader] Loaded {loaded.Length} prefabs from Resources/{resourcePath}");
        else
            Debug.LogWarning($"[PrefabAutoLoader] No prefabs found at Resources/{resourcePath}");

        return loaded;
    }
}
