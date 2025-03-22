using UnityEngine;

public class doorOpener : MonoBehaviour
{
    [SerializeField] Animator doorAnimator;

    void OnTriggerEnter(Collider character)
    {
        if (character.CompareTag("Player"))
        {
            if (doorAnimator != null) 
                doorAnimator.SetTrigger("Open");
            else 
                this.GetComponent<Animation>().Play("open");
        }
    }

    void OnTriggerExit(Collider character)
    {
        if (character.CompareTag("Player"))
            if (doorAnimator != null)
                doorAnimator.SetTrigger("Close");
            else
                this.GetComponent<Animation>().Play("close");
    }
}
