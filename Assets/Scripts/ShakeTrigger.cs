using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShakeTrigger : MonoBehaviour
{
    [Header("Explosion and Shake Settings")]
    public float triggerDistance = 10f;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.3f;
    public GameObject explosionEffectPrefab;

    private Transform playerTransform;
    private CameraShake cameraShake;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform; 
        }

        Camera mainCam = Camera.main;
        if (mainCam != null ) 
        {
            cameraShake = mainCam.GetComponent<CameraShake>();
        }
    }

    public void OnButtonPressed()
    {
        if (explosionEffectPrefab != null) 
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (playerTransform != null && Vector3.Distance(playerTransform.position, transform.position) <= triggerDistance)
        {
            if (cameraShake != null) 
            {
                cameraShake.Shake(shakeDuration, shakeIntensity);
            }
        }
    }
}
