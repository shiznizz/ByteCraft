using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    [Header("Player Base Stat")]

    public int HP;
    public int HPMax;

    public int Armor;
    public int ArmorMax;

    public float playerHeight;
    public float standingHeight = 2f;
    public float crouchHeight = 0.5f;
    public int playerHPMax = 100;
    public int playerHP;

    public int upgradeCurrency;

    [Header("Player Base Movement")]

    public float currSpeed;
    public float speedLimit;
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
    public float wallRunForce;
    public float maxWallRunTime;
    public float exitWallTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

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

    [Header("Magic Options")]
    public GameObject magicWeaponModel;
    public weaponStats startMagic;
    public GameObject magicProjectile; // Projectile Prefab
    public float magicProjectileSpeed; // Speed of projectile
    public Transform magicPosition;

    //bool isGunPOSSet;

    public float shootTimer;
    public float attackTimer;

    //Tracks which weapon is active
    public enum WeaponType { Gun, Melee, Magic }
    public WeaponType currentWeapon = WeaponType.Gun;
    //[Header("Starting Level bool")]
    //public bool isPlayerInStartingLevel;


    public void Awake()
    {
        instance = this;
    }

}
