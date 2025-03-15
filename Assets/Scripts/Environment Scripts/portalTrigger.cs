using System.Collections;
using TMPro;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    public GameObject portal;  // Drag the portal GameObject here in the inspector
    public float delayTime = 1f;  // Delay in seconds before the portal appears

   // [Header("Portal Name Settings")]
    //[SerializeField] TextMeshPro portalOneNameText;
    //[SerializeField] TextMeshPro portalTwoNameText;
    //[SerializeField] TextMeshPro portalThreeNameText;
    //[SerializeField] TextMeshPro portalFourNameText;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that enters is the player (assume the player has the tag "Player")
        if (other.CompareTag("Player"))
        {
            // Start the coroutine to activate the portal after the delay
            StartCoroutine(ActivatePortalWithDelay());
        }
    }

    private IEnumerator ActivatePortalWithDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayTime);

        // Activate the portal after the delay
        if (portal != null)
        {
            portal.SetActive(true);  // Activate the portal here
        //    portalOneNameText.gameObject.SetActive(true);
        //    portalTwoNameText.gameObject.SetActive(true);
        //    portalThreeNameText.gameObject.SetActive(true);
        //    portalFourNameText.gameObject.SetActive(true);
        }
    }
}
