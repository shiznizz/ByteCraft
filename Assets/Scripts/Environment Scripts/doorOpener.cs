using UnityEngine;

public class doorOpener : MonoBehaviour
{
    [SerializeField] Animator doorAnimator;
    [SerializeField] GameObject dependentObject;

    void OnTriggerEnter(Collider character)
    {
        if (character.CompareTag("Player") || character.CompareTag("Enemy"))
        {
            if (dependentObject == null || dependentObject.activeSelf == true)
            {
                if (doorAnimator != null)
                    doorAnimator.SetTrigger("Open");
                else
                    this.GetComponent<Animation>().Play("open");
            }
        }
    }

    void OnTriggerExit(Collider character)
    {
        if (dependentObject == null || dependentObject.activeSelf == true)
        {
            if (character.CompareTag("Player"))
                if (doorAnimator != null)
                    doorAnimator.SetTrigger("Close");
                else
                    this.GetComponent<Animation>().Play("close");
        }
    }
}
