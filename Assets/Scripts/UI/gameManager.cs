using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.Diagnostics.Contracts;


public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("Menus")]
    [SerializeField] GameObject menuInventory;
    public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDeath;
    [SerializeField] GameObject menuObjectiveFail;
    [SerializeField] TMP_Text objectiveText;


    [Header("UI Elements to Toggle Visibility")]
    [SerializeField] GameObject ammoHUD;
    [SerializeField] GameObject jetpackHUD;
    [SerializeField] GameObject enemyHealthbar;
    public Image playerHPBar;
    public Image enemyHPBar;
    public Image JPFuelGauge;
    public Image grappleGauge;
    public GameObject playerDamageScreen;
    public GameObject checkpointPopup;

    [Header("Text Fields to Update")]
    [SerializeField] public TMP_Text goalCountText;
    [SerializeField] public TMP_Text ammoCurText;
    [SerializeField] public TMP_Text ammoMaxText;
    [SerializeField] public TMP_Text ammoReserveText;

    [Header("State Monitoring Values")]
    public bool isPaused;
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;

    int goalCount;

    [Header("Inventory Options")]
    [SerializeField] GameObject inventorySlot;
    public GameObject selectedEquipSlot;
    public GameObject selectedInventorySlot;

    public GameObject[] slots;

    public Image itemIcon;

    public TMP_Text deleteNotifaction;
    public TMP_Text itemDescription;
    public TMP_Text itemName;

    public GameObject displaySlot;

    [Header("Low Health Screen Indicator")]
    public Image lowHealthIndicator;

    [SerializeField] float lowHealthThreshold = 0.25f;
    [SerializeField] float heartbeatSpeed = 2f;
    [SerializeField] float heartbeatMagnitude = 0.2f;
    [SerializeField] float baseAlpha = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
    }

    private void Start()
    {
        updateInventory();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
                switchMenu(menuPause);
            else
                stateUnpause();
        }
        if (Input.GetButtonDown("Inventory"))
        {
            switchMenu(menuInventory);
        }

        //CheckLowHealth();
    }

    #region Menus

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void switchMenu(GameObject menuToOpen, bool closeMenu = true)
    {
        if (menuActive == null)
        {
            statePause();
            menuActive = menuToOpen;
            menuActive.SetActive(true);
        }
        else if (closeMenu && menuActive == menuToOpen)
        {
            stateUnpause();
        }
        else
        {
            menuActive.SetActive(false);
            menuActive = menuToOpen;
            menuActive.SetActive(true);
        }

    }

    public void youLose()
    {
        switchMenu(menuDeath);
    }

    public void objectiveFailed(string failedObj)
    {
        switchMenu(menuObjectiveFail);
        objectiveText.SetText(failedObj);
    }

    public void youWin()
    {
        switchMenu(menuWin);
    }

    #endregion Menus

    #region UI Element Updates
    public void updateGameGoal(int amount)
    {
        goalCount += amount;
        goalCountText.text = goalCount.ToString("F0");

        if (goalCount <= 0)
        {
            youWin();
        }
    }

    public void updateAmmo()
    {
        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();
        ammoCurText.text = gun.ammoCur.ToString("D3");
        ammoMaxText.text = gun.ammoMax.ToString("D3");
        ammoReserveText.text = gun.ammoReserve.ToString("D3");
    }

    public void hideAmmo()
    {
        ammoHUD.SetActive(false);
    }

    public void showAmmo()
    {
        ammoHUD.SetActive(true);
    }

    public void showJetpack()
    {
        jetpackHUD.SetActive(true);
    }

    public void hideJetpack()
    {
        jetpackHUD.SetActive(false);
    }

    private void CheckLowHealth()
    {
        // Check if playerStatManager or lowHealthIndicator is null to avoid NullReferenceException
        if (playerStatManager.instance == null || lowHealthIndicator == null) return;

        if (playerStatManager.instance.HPMax <= 0) return;

        float hpRatio = (float)playerStatManager.instance.HP / playerStatManager.instance.HPMax;

        if (hpRatio <= lowHealthThreshold)
        {
            float alpha = baseAlpha + Mathf.Sin(Time.time * heartbeatSpeed) * heartbeatMagnitude;
            alpha = Mathf.Clamp01(alpha);

            Color c = lowHealthIndicator.color;
            c.a = alpha;
            lowHealthIndicator.color = c;  
        }
        else
        {
            Color c = lowHealthIndicator.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime);
            lowHealthIndicator.color = c;
        }
    }

    #endregion UI Element Updates

    #region Inventory
    public void updateInventory()
    {

        for (int i = 0; i < slots.Length; i++)
        {

            if (inventoryManager.instance.inventory.Count == 0)
            {
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                slots[i].GetComponent<SlotBoss>().isFull = false;
                slots[i].GetComponent<SlotBoss>().item = null;
            }

            try
            {
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = inventoryManager.instance.inventory[i].itemIcon;
                slots[i].GetComponent<SlotBoss>().item = inventoryManager.instance.inventory[i];
                slots[i].GetComponent<SlotBoss>().isFull = true;

            }
            catch
            {
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                slots[i].GetComponent<SlotBoss>().isFull = false;
                slots[i].GetComponent<SlotBoss>().item = null;

            }
        }
    }

    public void deselectSlot()
    {
        if (selectedEquipSlot != null)
        {
            selectedEquipSlot.GetComponent<equipSlot>().isSelected = false;
            selectedEquipSlot.transform.GetChild(1).gameObject.SetActive(false);
        }

        if(selectedInventorySlot != null)
        {
            selectedInventorySlot.GetComponent<SlotBoss>().isSelected = false;
            selectedInventorySlot.transform.GetChild(2).gameObject.SetActive(false);
        }

        itemDescription.text = "";
        itemName.text = "";
        displaySlot.SetActive(false);
    }
    #endregion Inventory
}
