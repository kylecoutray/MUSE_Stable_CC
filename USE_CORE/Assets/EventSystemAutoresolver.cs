using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class EventSystemAutoresolver : MonoBehaviour
{
    private static EventSystemAutoresolver instance;

    void Awake()
    {
        // Singleton pattern: only one should exist
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ResolveDuplicatesAfterLoad());
    }

    IEnumerator ResolveDuplicatesAfterLoad()
    {
        // Wait one frame to allow objects to finish initializing
        yield return null;

        for (int i = 0; i < 10; i++)
        {
            var systems = FindObjectsOfType<EventSystem>(true);

            if (systems.Length > 1)
            {
                foreach (var es in systems)
                {
                    if (es != EventSystem.current)
                    {
                        Debug.LogWarning($"[AutoResolver] Destroying extra EventSystem: {es.name}", es.gameObject);
                        Destroy(es.gameObject);
                    }
                }
            }

            if (systems.Length <= 1)
                break;

            yield return null;
        }
    }
}
