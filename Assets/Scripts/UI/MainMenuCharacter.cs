using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;  // Speed of movement
    [SerializeField] private Vector3 movementDirection = Vector3.forward;  // Direction in which the character moves

    private void Update()
    {
        MoveCharacterInPlace();
    }

    void MoveCharacterInPlace()
    {
        // Move the character in a set direction
        transform.Translate(movementDirection * moveSpeed * Time.deltaTime);
    }
}
