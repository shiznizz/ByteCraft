using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    [Header("Player Base Stat")]

    public int HP;
    public int HPMax;

    public float playerHeight;
    public float standingHeight = 2f;
    public float crouchHeight = 0.5f;
    public int playerHPMax = 100;
    public int playerHP;

    public int upgradeCurrency;

    [Header("Player Shield Stat")]
    public float shield;
    public float shieldMax;
    public float shieldOverChargeMax;
    public float shieldRegen;
    public float shieldRegenDelay;


    [Header("Player Base Movement")]

    public float currSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeedMod;
    public float jetpackAirMod;

    [Header("Player Base Physics/gravity")]

    public float groundDrag;
    public float airDrag;
    public float drag;
    public float gravity;

    [Header("Player Base Jump")]

    public float jumpForce;
    public float jumpMax;
    public float jumpCount;

    [Header("JetPack Stats")]
    public bool hasJetpack;
    public bool hasGroundCheck = false;
    public int jetpackFuelMax;
    public float jetpackFuel;
    public float jetpackFuelUse;
    public float jetpackFuelRegen;
    public float jetpackFuelRegenDelay;
    public int jetpackSpeed;
    public float jetpackHoldTimer = 0.01f;

    [Header("Slide Stats")]
    public float maxSlideTime;
    public float slideSpeed;
    public float slideSpeedMax;
    public float slideFriction;

    [Header("Wall Run Stats")]
    public float wallRunSpeed;
    public float wallAdhesiveForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    public float exitWallTime;
    public float wallRerunTime;
    public float wallCheckDistance;
    public float minimumWallAngleDifference;

    [Header("Grapple Options")]
    public int grappleDistance;
    public int grappleLift;
    public float grappleSpeedMultiplier;
    public float grappleSpeedMin;
    public float grappleSpeedMax;
    public float grappleCooldown;

    [Header("Grapple Gun")]
    public Transform grappleShootPos;
    public LineRenderer grappleRope;

    [Header("Common Weapon Options")]
    public float attackCooldown;
    public int attackDamage;
    public int attackDistance;
    public int attackRange;

    [Header("Range Options")]
    public GameObject gunModel;
    public weaponStats startGun;
    public Transform muzzleFlash;

    [Header("Melee Options")]
    public Transform meleePos;
    public GameObject meleeWeaponModel;
    public weaponStats startMelee;
    public Animator playerAnimator;
    public Collider meleeCol;

    //bool isGunPOSSet;

    public float shootTimer;
    public float attackTimer;

    //variables for upgrade tracking
    private float origSprintSpeed;
    private int curSprintMod;
    private float origJetpackRegen;
    private int curJetpackRegenMod;
    private int origHPMax;
    private int curHPMaxMod;
    private float origShieldMax;
    private int curShieldMaxMod;


    public void Awake()
    {
        instance = this;
        origHPMax = playerHPMax;
        origShieldMax = shieldMax;
        origSprintSpeed = sprintSpeed;
        origJetpackRegen = jetpackFuelRegenDelay;
    }

    public void increaseSprintSpeed(int percentToIncrease)
    {
        sprintSpeed = origSprintSpeed * (100 + curSprintMod + percentToIncrease) / 100;
        curSprintMod += percentToIncrease;
    }

    public void increaseJetpackRegen(int percentToIncrease)
    {
        jetpackFuelRegenDelay = origJetpackRegen * (100 + curJetpackRegenMod + percentToIncrease) / 100;
        curJetpackRegenMod += percentToIncrease;
    }

    public void increaseMaxHealth(int percentToIncrease)
    {
        playerHPMax = origHPMax * (100 + curHPMaxMod + percentToIncrease) / 100;
        curHPMaxMod += percentToIncrease;
    }

    public void increaseMaxShield(int percentToIncrease)
    {
        shieldMax = origShieldMax * (100 + curShieldMaxMod + percentToIncrease) / 100;
        curShieldMaxMod += percentToIncrease;
    }
}
