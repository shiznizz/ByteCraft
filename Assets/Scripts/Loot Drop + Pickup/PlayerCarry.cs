using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    // set this in the inspector to an empty GameObject on player
    public Transform carryPoint;
    public float pickupRange = 3f;
    private CarryableObjects currentCarry;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // check if player presses E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentCarry == null)
            {
                // attempt to pick up nearby carryable object using raycast from center of screen
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
                {
                    CarryableObjects carryable = hit.collider.GetComponent<CarryableObjects>();
                    if (carryable != null && !carryable.IsCarried)
                    {
                        carryable.PickUp(carryPoint);
                        currentCarry = carryable;
                        Debug.Log("Picked Up: " + carryable.name);
                    }
                }
            }
            else
            {
                // drop current carried obj
                currentCarry.Drop();
                currentCarry = null;
                Debug.Log("Dropped the Object.");
            }
        }
    }
}
