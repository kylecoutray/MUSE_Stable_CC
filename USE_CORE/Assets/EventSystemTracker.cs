using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemTracker : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning($"[EventSystemTracker] EventSystem created: {name}", this);
        Debug.Log("[EventSystemTracker] Stack trace:\n" + System.Environment.StackTrace);
    }
}
