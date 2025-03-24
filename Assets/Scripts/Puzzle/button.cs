using UnityEngine;
using UnityEngine.UI;

public class buttons : MonoBehaviour
{
    // when player is in range of button and presses E while looking at button
    // tap buttons vs hold buttons
    // floor buttons (similar to switches)

    // what is being toggled
    // how does toggling it effect the object or event

    [Header("Button Settings")]
    [SerializeField] Transform buttonPosition;
    [SerializeField] SphereCollider buttonRadius;
    [SerializeField] GameObject[] objectsToActivate;
    [SerializeField] float activationRange;
    [SerializeField] float holdDuration;
    public bool isHoldButton;
    public bool playerInRange;
    public bool isActivated;
    public bool isHolding;
    public float holdTime;
    bool isMarked;

    [Header("Visuals & Audio")]
    [SerializeField] Renderer buttonModel;
    private Color colorActive;
    private Color colorInactive;
    [SerializeField] Image holdBarFill;
    [SerializeField] Canvas holdBar;
    [SerializeField] GameObject buttonPrompt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonRadius = GetComponent<SphereCollider>();
        buttonRadius.radius = activationRange;
        colorInactive = buttonModel.material.color;
        colorActive = Color.green;
        buttonPrompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isHolding)
        {
            holdBar.gameObject.SetActive(true);
            UpdateHoldBar();
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
            holdBar.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if you have to leave and come back to activate multiple times
        if (other.CompareTag("Player"))
        {
            buttonPrompt.SetActive(true);
            holdTime = 0;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            buttonPrompt.SetActive(false);
            holdTime = 0;
            isHolding = false;
            playerInRange = false;
        }
    }

    public void toggleButton()
    {
        isActivated = !isActivated;
        buttonModel.material.color = isActivated ? colorActive : colorInactive;
        foreach(GameObject obj in objectsToActivate)
            obj.SetActive(!obj.activeSelf);

        if (!isMarked)
        {
            isMarked = true;
            GameEventsManager.instance.buttonPressEvents.ButtonGained(1);
            GameEventsManager.instance.miscEvents.ButtonPressed();
        }
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

    void UpdateHoldBar()
    {
        holdBarFill.fillAmount = (float)holdTime / holdDuration;
    }
}
