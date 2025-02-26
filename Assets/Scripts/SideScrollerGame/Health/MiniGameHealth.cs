using System.Collections;
using UnityEngine;

public class MiniGameHealth : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // InvulnerabilityDuration
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    [Header("Components")]
    [SerializeField] private Behaviour[] components;
    private bool invulnerable;


    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();

        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
        
        if(currentHealth > 0)
        {
            anim.SetTrigger("hurt");
            StartCoroutine(Invunerability());
        }
        else
        {
            if (!dead)
            {
                //Deactivate all attached component classes
                foreach (Behaviour component in components)
                    component.enabled = false;

                anim.SetTrigger("die");
                GetComponent<SideScrollerPlayerController>().enabled = false;
                dead = true;
            }
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    public void Respawn()
    {

        dead = false;
        AddHealth(startingHealth);
        anim.ResetTrigger("die");
        anim.Play("sideScrollerPlayerIdle");
        StartCoroutine(Invunerability());

        //Activate all attached component classes
        foreach (Behaviour component in components)
            component.enabled = true;

        // Re-enable player movement
        GetComponent<SideScrollerPlayerController>().enabled = true;
    }

    private IEnumerator Invunerability()
    {
        Physics2D.IgnoreLayerCollision(3, 7, true);

        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));

        }

        Physics2D.IgnoreLayerCollision(3, 7, false);
    }
}
