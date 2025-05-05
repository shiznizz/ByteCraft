using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static Vector3 positionOffset = Vector3.zero;
    public static Quaternion rotationOffset = Quaternion.identity;
    private bool isShaking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Awake()
    //{
    //    originalPos = transform.localPosition;
    //    originalRot = transform.localRotation;
    //}

    // call to initiate a cam shake
    public void Shake(float duration, float intensity)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, intensity));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        float elapsed = 0f;
        while (elapsed < duration) 
        {
            positionOffset = Random.insideUnitSphere * intensity;

            //// generate random offset for pos
            //Vector3 randomPos = Random.insideUnitSphere * intensity; 
            //positionOffset = randomPos;

            // apply small random rotation offset around each axis
            Vector3 randomRot = new Vector3
                (Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity));
            rotationOffset = Quaternion.Euler(randomRot);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // reset to og pos and rotation
        positionOffset = Vector3.zero;
        rotationOffset = Quaternion.identity;
        isShaking = false; 
    }
}
