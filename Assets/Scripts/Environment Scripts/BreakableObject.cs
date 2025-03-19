using System.Collections;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IDamage
{
    [SerializeField] int objectHP;
    [SerializeField] GameObject wholeObject;
    [SerializeField] GameObject fracturedObject;
    [SerializeField] float seperationForce;
    [SerializeField] float removeFromWorldTime;
    [SerializeField] bool isMainObjective;
    [SerializeField] string failedText;

    private Color originalColor;
    private float originalObjectHP;

    void Start()
    {
        originalColor = wholeObject.GetComponent<Renderer>().material.color;
        originalObjectHP = objectHP;
        if (isMainObjective)
        {
            gameManager.instance.enemyHealthbar.SetActive(true);
            gameManager.instance.enemyHPBar.color = Color.green;
            gameManager.instance.enemyHPBar.fillAmount = (float)objectHP / originalObjectHP;
        }

    }

    public void takeDamage(int amount)
    {
        objectHP -= amount;

        if (objectHP <= 0)
        {
            BreakObject();
            if (isMainObjective)
            {
                gameManager.instance.objectiveFailed(failedText);
                gameManager.instance.enemyHPBar.color = Color.red;
            }
        }
        else
        {
            StartCoroutine(colorFlash());
        }

        if (isMainObjective)
        {
            gameManager.instance.enemyHPBar.fillAmount = (float) objectHP / originalObjectHP;
        }
    }

    private void BreakObject()
    {
        wholeObject.SetActive(false);
        this.GetComponent<Collider>().enabled = false;
        GameObject createdObject = Instantiate(fracturedObject, transform.position, transform.rotation);

        foreach (Rigidbody rb in createdObject.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 force = (rb.transform.position - transform.position).normalized * seperationForce;
            rb.AddForce(force);
        }

        StartCoroutine(RemovePieces(createdObject));
    }

    IEnumerator RemovePieces(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(removeFromWorldTime);
        Destroy(objectToDestroy);
    }

    IEnumerator colorFlash()
    {
        wholeObject.GetComponent<Renderer>().material.color = Color.gray;
        yield return new WaitForSeconds(0.1f);
        wholeObject.GetComponent<Renderer>().material.color = originalColor;
    }
}
