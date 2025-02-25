using UnityEngine;

public class MiniGameHealthCollectible : MonoBehaviour
{
    [SerializeField] private float healthValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<MiniGameHealth>().AddHealth(healthValue);
            gameObject.SetActive(false);
        }
    }
}
