using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FloatingDamageText : MonoBehaviour
{
    public float lifetime = 3f;
    public float riseSpeed = 1f;
    public TextMeshPro textMesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshPro>();

        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
