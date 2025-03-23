using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] GameObject[] requiredObjects;
    [SerializeField] float amountToMoveX;
    [SerializeField] float amountToMoveY;
    [SerializeField] float amountToMoveZ;

    private bool hasMoved;

    // Update is called once per frame
    void Update()
    {
        if (allActive() && !hasMoved)
        {
            transform.position = new Vector3(transform.position.x + amountToMoveX, transform.position.y + amountToMoveY, transform.position.z + amountToMoveZ);
            hasMoved = true;
        }
    }

    private bool allActive()
    {
        foreach (GameObject obj in requiredObjects)
        {
            if (obj.activeSelf == false)
                return false;
        }

        return true;
    }
}
