using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    public float lifetime = 3f; // text will disappear after 3 secs
    public float riseSpeed = 1f; // how fast the text moves upwards
    public TextMeshPro textMesh; // reference to TMP component

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if not set via inspector, get TMP from children
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshPro>();

        // auto destroy obj after lifetime ends
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        // move text upward
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // rotate so it faces camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }

    // allows setting the displayed dmg val
    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
