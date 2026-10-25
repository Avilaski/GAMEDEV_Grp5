
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneMovement : MonoBehaviour
{
    [Header("Lane Positions (World X)")]
    [SerializeField] private float leftLaneX = -2.33208f;
    [SerializeField] private float middleLaneX = 0.18792f;
    [SerializeField] private float rightLaneX = 2.78792f;

    [Header("Movement")]
    [SerializeField] private float laneChangeSpeed = 6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController characterController;
    private float targetX;
    private bool isChangingLanes;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        targetX = transform.position.x;
    }

    private void Update()
    {
        ReadLaneInput();
        MoveToLane();
        UpdateAnimation();
    }

    private void ReadLaneInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            targetX = leftLaneX;
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            targetX = middleLaneX;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            targetX = rightLaneX;
        }

        isChangingLanes =
            Mathf.Abs(transform.position.x - targetX) > 0.01f;
    }

    private void MoveToLane()
    {
        float currentX = transform.position.x;

        float nextX = Mathf.MoveTowards(
            currentX,
            targetX,
            laneChangeSpeed * Time.deltaTime
        );

        float deltaX = nextX - currentX;

        // Move only along the world X-axis.
        // Y and Z remain unchanged.
        if (characterController != null)
        {
            characterController.Move(
                new Vector3(deltaX, 0f, 0f)
            );
        }
        else
        {
            transform.position += new Vector3(deltaX, 0f, 0f);
        }

        isChangingLanes =
            Mathf.Abs(transform.position.x - targetX) > 0.01f;
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isChangingLanes);
        }
    }
}
