using System.Collections;
using TMPro.Examples;
using UnityEngine;

public class MiniGamePlayerRespawn : MonoBehaviour
{
    //[SerializeField] private AudioClip checkpointSound; 
    private Transform currentCheckpoint;
    private MiniGameHealth playerHealth;
    private MiniGameUIManager uiManager;

    private void Awake()
    {
        playerHealth = GetComponent<MiniGameHealth>();
        uiManager = FindObjectOfType<MiniGameUIManager>();
    }

    public void checkRespawn()
    {
        //Check if checkpoint is available
        if (currentCheckpoint == null)
        {
            //Show game over screen
            uiManager.GameOver();

            return;
        }

        playerHealth.Respawn();
        transform.position = currentCheckpoint.position;
              
        //Move camera back to player
        Camera.main.GetComponent<MiniGameCameraController>().MoveToNewRoom(currentCheckpoint.parent);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Checkpoint")
        {
            currentCheckpoint = collision.transform; 
            
            collision.GetComponent<Collider2D>().enabled = false;
            collision.GetComponent<Animator>().SetTrigger("appear");
        }
    }
}
