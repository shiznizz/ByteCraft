using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class CarryableObjects : MonoBehaviour
{

    private Rigidbody rb;
    public bool IsCarried {  get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // called when player picks up object
    public void PickUp(Transform carryPoint)
    {
        IsCarried = true;
        rb.isKinematic = true; // disable physics while carried
        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero; // adjust the offset as needed
        transform.localRotation = Quaternion.identity;
    }

    // call when player drops object
    public void Drop()
    {
        IsCarried = false;
        transform.SetParent(null); 
        rb.isKinematic = false; // reenable physics when dropped
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
