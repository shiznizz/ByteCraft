using UnityEngine;

public class buttonManager : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            this.gameObject.SetActive(false);
        }
    }
}
