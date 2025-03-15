using UnityEngine;
using UnityEngine.SceneManagement;

public class doNotDestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            DontDestroyOnLoad(gameObject);
    }
}
