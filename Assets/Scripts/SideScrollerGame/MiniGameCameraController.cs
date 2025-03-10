using UnityEngine;

public class MiniGameCameraController : MonoBehaviour
{
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;
    private bool isReadyForRoomChange = false;

    private void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z), ref velocity, speed);
    }

    public void MoveToNewRoom(Transform _newRoom)
    {
        currentPosX = _newRoom.position.x;

        isReadyForRoomChange = true;
    }

    public void ResetRoomChange()
    {
        isReadyForRoomChange = false;  // Prevent camera from moving until it's allowed again
    }
}
