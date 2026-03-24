using System;

/// <summary>
/// Centrální statický event systém. Voláno kdekoliv v projektu bez reference na konkrétní objekt.
/// </summary>
public static class EventBus
{
    public static event Action<string> OnMessageRead;
    public static event Action<string> OnArtifactReported;
    public static event Action<string> OnHypothesisBuilt;
    public static event Action<string> OnPolygonUnlocked;
    public static event Action<int>    OnPrestigeChanged;

    public static void MessageRead(string id)        => OnMessageRead?.Invoke(id);
    public static void ArtifactReported(string id)  => OnArtifactReported?.Invoke(id);
    public static void HypothesisBuilt(string id)   => OnHypothesisBuilt?.Invoke(id);
    public static void PolygonUnlocked(string id)   => OnPolygonUnlocked?.Invoke(id);
    public static void PrestigeChanged(int newValue) => OnPrestigeChanged?.Invoke(newValue);

    /// <summary>
    /// Odhlásí všechny handlery — volej při změně scény pokud to potřebuješ.
    /// </summary>
    public static void ClearAllListeners()
    {
        OnMessageRead      = null;
        OnArtifactReported = null;
        OnHypothesisBuilt  = null;
        OnPolygonUnlocked  = null;
        OnPrestigeChanged  = null;
    }
}
