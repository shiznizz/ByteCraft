using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;


public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("UI Elements to Toggle Visibility")]
    [SerializeField] GameObject menuInventory;
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject ammoHUD;
    [SerializeField] GameObject jetpackHUD;

    public Image playerHPBar;
    public Image JPFuelGauge;
    public Image grappleGauge;
    public GameObject playerDamageScreen;
    public GameObject checkpointPopup;

    public GameObject selectedEquipSlot;
    public GameObject selectedInventorySlot;

    [Header("Text Fields to Update")]
    [SerializeField] TMP_Text goalCountText;
    [SerializeField] TMP_Text ammoCurText;
    [SerializeField] TMP_Text ammoMaxText;
    [SerializeField] TMP_Text ammoReserveText;

    [Header("State Monitoring Values")]
    public bool isPaused;
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;

    int goalCount;

    [Header("Inventory Options")]
    [SerializeField] GameObject inventorySlot;

    public GameObject[] slots;

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
            if(menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if(menuActive == menuPause)
            {
                stateUnpause();
            }
        }
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryMenu();
        }
    }

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

    public void inventoryMenu()
    {

        if (menuActive == null)
        {
            statePause();
            menuActive = menuInventory;
            menuActive.SetActive(true);
        }
        else if (menuActive == menuInventory)
        {
            stateUnpause();
        }
    }


    public void updateGameGoal(int amount)
    {
        goalCount += amount;
        goalCountText.text = goalCount.ToString("F0");

        if (goalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void updateAmmo(gunStats gun)
    {
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

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

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
        
        
    }
}
