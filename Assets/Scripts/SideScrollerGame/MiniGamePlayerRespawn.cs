using System.Collections;
using TMPro.Examples;
using UnityEngine;

public class MiniGamePlayerRespawn : MonoBehaviour
{
    //[SerializeField] private AudioClip checkpointSound; 
    private Transform currentCheckpoint;
    private MiniGameHealth playerHealth;
    //[SerializeField] private Transform checkpointRoom;

    private void Awake()
    {
        playerHealth = GetComponent<MiniGameHealth>();
    }

    public void Respawn()
    {
        playerHealth.Respawn();
        transform.position = currentCheckpoint.position;
        

        //Camera.main.transform.position = new Vector3(currentCheckpoint.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

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
