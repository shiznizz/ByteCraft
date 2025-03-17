using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController playerScript = other.GetComponent<playerController>();
            if (playerScript != null)
            {
                playerScript.MoveController(destination);
            }
        }
    }
}
