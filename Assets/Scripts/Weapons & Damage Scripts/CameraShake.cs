using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private Quaternion originalRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    // call to initiate a cam shake
    public void Shake(float duration, float intensity)
    {
        StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration) 
        {
            // generate random offset for pos
            Vector3 randomPos = originalPos + Random.insideUnitSphere * intensity; 
            transform.localPosition = randomPos;

            // apply small random rotation offset around each axis
            Vector3 randomRot = new Vector3
                (Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity));
            transform.localRotation = Quaternion.Euler(originalRot.eulerAngles + randomRot);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // reset to og pos and rotation
        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
