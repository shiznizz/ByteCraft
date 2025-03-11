using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;
    private float fallingTimer;

    [SerializeField] private bool isStartFalling;

    float rotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        fallingTimer = 10f;
        if (isStartFalling)
            fallingTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        fallingTimer += Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;
            

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        if(fallingTimer > 2.3f)
        {
            transform.localRotation = Quaternion.Euler(rotX, 0, 0);

            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }
}