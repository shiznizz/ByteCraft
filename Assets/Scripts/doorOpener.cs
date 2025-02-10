using UnityEngine;

public class doorOpener : MonoBehaviour
{
    [SerializeField] Animator doorAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OnTriggerEnter(Collider character)
    {
        if (character.CompareTag("Player"))
            doorAnimator.SetTrigger("Open");
    }

    void OnTriggerExit(Collider character)
    {
        if (character.CompareTag("Player"))
            doorAnimator.SetTrigger("Close");
    }
}
