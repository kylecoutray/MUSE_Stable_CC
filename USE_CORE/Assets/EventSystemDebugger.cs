using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemDebugger : MonoBehaviour
{
    private static bool isPrimary = false;

    void Update()
    {
        // Only allow one active EventSystem
        var allEventSystems = FindObjectsOfType<EventSystem>(true);

        if (allEventSystems.Length > 1)
        {
            foreach (var es in allEventSystems)
            {
                if (es != EventSystem.current)
                {
                    Debug.LogWarning($"Destroying extra EventSystem: {es.name}", es.gameObject);
                    Destroy(es.gameObject);
                }
            }
        }

        // Once it's resolved, destroy this limiter (optional optimization)
        if (allEventSystems.Length <= 1)
        {
            Destroy(this); // Remove the script from the object
        }
    }
}
