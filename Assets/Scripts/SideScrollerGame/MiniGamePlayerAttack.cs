using UnityEngine;

public class MiniGamePlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    //[SerializeField] private GameObject[] fireballs;
    [SerializeField] private GameObject fireballPrefab;

    private Animator anim;
    private SideScrollerPlayerController playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<SideScrollerPlayerController>();
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;
        Debug.Log("Cooldown Timer: " + cooldownTimer);  // Debugging cooldown timer


        if (Input.GetMouseButtonDown(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            Attack();
        }
    }

    private void Attack()
    {
        anim.SetTrigger("attack");
        cooldownTimer = 0;

        // Instantiate a new fireball at the firePoint position
        GameObject newFireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        // Set the direction of the newly instantiated fireball
        newFireball.GetComponent<MiniGameProjectile>().SetDirection(Mathf.Sign(transform.localScale.x));

        //fireballs[FindFireball()].transform.position = firePoint.position;
        //fireballs[FindFireball()].GetComponent<MiniGameProjectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }

    //private int FindFireball()
    //{
    //    for (int i = 0; i < fireballs.Length; i++)
    //    {
    //        if (!fireballs[i].activeInHierarchy)
    //            return i;
    //    }
    //    return 0;
    //}
}
