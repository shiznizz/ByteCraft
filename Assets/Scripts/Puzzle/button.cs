using System.Collections;
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
    public enum buttonType
    {
        toggle, hold
    }
    public buttonType buttonT;

    [SerializeField] float holdDuration;
    public bool isPermanentButton;
    public bool playerInRange;
    public bool isActivated;
    public bool isHolding;
    public float holdTime;
    public bool dontToggleFlash;
    bool isMarked;
    bool hasToggled = false;

    Coroutine flashButtonCor;

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

        if (buttonPrompt != null) 
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
        else
        {
            holdBar.gameObject.SetActive(false);
        }
    }

    public void ReleaseButton()
    {
        if (this.buttonT == buttonType.hold)
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
            if (buttonPrompt != null) 
                buttonPrompt.SetActive(true);

            holdTime = 0;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buttonPrompt != null)
                buttonPrompt.SetActive(false);

            holdTime = 0;
            isHolding = false;
            playerInRange = false;
        }
    }

    public void toggleButton()
    {
        if(!isPermanentButton)
            isActivated = !isActivated; 
        else if (isPermanentButton && !hasToggled)
        {
            isActivated = !isActivated;
            hasToggled = true;
        }

        if (isPermanentButton || dontToggleFlash)
            buttonModel.material.color = isActivated ? colorActive : colorInactive;
        else
            if (flashButtonCor == null)
                flashButtonCor = StartCoroutine(flashButtonColor());

            foreach (GameObject obj in objectsToActivate)
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
        if (this.buttonT == buttonType.hold)
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

    IEnumerator flashButtonColor()
    {
        try
        {
            buttonModel.material.color = colorActive;
            yield return new WaitForSeconds(0.5f);
            buttonModel.material.color = colorInactive;
        }
        finally
        {
            flashButtonCor = null;
        }
    }
}
