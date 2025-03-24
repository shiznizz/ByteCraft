using UnityEngine;

public class buttonManager : MonoBehaviour
{
    public static buttonManager Instance;

    public GameObject mainQuitButton;
    public GameObject pauseQuitButton;
    public GameObject deathQuitButton;
    public GameObject failQuitButton;
    public GameObject winQuitButton;

    private void Awake()
    {
        Instance = this;
    }
}
