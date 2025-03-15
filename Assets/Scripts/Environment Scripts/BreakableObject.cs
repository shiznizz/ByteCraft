using System.Collections;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IDamage
{
    [SerializeField] int objectHP;
    [SerializeField] GameObject wholeObject;
    [SerializeField] GameObject fracturedObject;
    [SerializeField] float seperationForce;
    [SerializeField] float removeFromWorldTime;

    private Color originalColor;

    void Start()
    {
        originalColor = wholeObject.GetComponent<Renderer>().material.color;
    }


    public void takeDamage(int amount)
    {
        objectHP -= amount;

        if (objectHP <= 0)
        {
            BreakObject();
        }
        else
        {
            StartCoroutine(colorFlash());
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
