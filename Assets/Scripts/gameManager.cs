using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text goalCountText;
    [SerializeField] GameObject ammoHUD;
    [SerializeField] public TMP_Text ammoCurText;
    [SerializeField] public TMP_Text ammoMaxText;
    [SerializeField] public TMP_Text ammoReserveText;

    public Image playerHPBar;
    public Image JPFuelGauge;
    public Image grappleGauge;
    public GameObject playerDamageScreen;
    public bool isPaused;
    public GameObject player;
    public playerController playerScript;

    int goalCount;

    public GameObject playerSpawnPos;
    public GameObject checkpointPopup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
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

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
