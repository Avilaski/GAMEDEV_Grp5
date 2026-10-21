using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Gravity")]
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            input = new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0
            );
        }

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        if (movement.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}