using UnityEngine;

public class buttons : MonoBehaviour
{
    // when player is in range of button and presses E while looking at button
    // tap buttons vs hold buttons
    // floor buttons (similar to switches)

    // what is being toggled
    // how does toggling it effect the object or event

    [Header("Button Settings")]
    [SerializeField] Transform buttonPosition;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] SphereCollider buttonRadius;
    [SerializeField] GameObject objectToActivate;
    [SerializeField] float activationRange;
    [SerializeField] float holdDuration;
    public bool playerInRange;
    public bool isHoldButton;
    public bool isActivated;
    public bool isHolding;
    public float holdTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonRadius = GetComponent<SphereCollider>();
        buttonRadius.radius = activationRange;
    }

    // Update is called once per frame
    void Update()
    {
        if(isHolding)
        {
            holdTime += Time.deltaTime;

            if (holdTime >= holdDuration)
            {
                toggleButton();
                isHolding = false;
                holdTime = 0;
            }
        }
    }

    public void ReleaseButton()
    {
        if (isHoldButton)
        {
            isHolding = false;
            holdTime = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if you have to leave and come back to activate multiple times
        if (other.CompareTag("Player"))
        {
            holdTime = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            holdTime = 0;
            isHolding = false;
        }
    }

    public void toggleButton()
    {
        isActivated = !isActivated;
        objectToActivate.SetActive(isActivated);
        Debug.Log("Button Toggled: " + isActivated);
    }

    public void pressButton()
    {
        if (isHoldButton)
        {
            isHolding = true;
            holdTime = 0;
        }
        else
            toggleButton();
    }
}
