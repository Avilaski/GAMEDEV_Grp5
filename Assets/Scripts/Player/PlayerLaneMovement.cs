using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerLaneMovement : MonoBehaviour
{
    [Header("Lane Positions (World X)")]
    [SerializeField]
    private float leftLaneX = -2.33208f;
    [SerializeField]
    private float middleLaneX = 0.18792f;
    [SerializeField]
    private float rightLaneX = 2.78792f;
    [Tooltip("Current scene lanes, in left/middle/right order. Their world positions override the old branch's coordinates.")]
    [SerializeField]
    private Transform[] laneReferences = new Transform[3];
    private int targetLane = 1;
    private PlayerHealth playerHealth;
    private PlayerCrateEffects crateEffects;

    private float LaneX(int index)
    {
        if (laneReferences != null && index < laneReferences.Length && laneReferences[index] != null)
        {
            return laneReferences[index].position.x;
        }

        return index == 0 ? leftLaneX : index == 1 ? middleLaneX : rightLaneX;
    }

    [Header("Movement")]
    [SerializeField]
    private float laneChangeSpeed = 20f;

    [Header("Bomb Protection")]
    [Min(0f)]
    [SerializeField]
    private float bombProtectionAfterArrival = 0.1f;
    private float bombProtectionUntil = float.NegativeInfinity;
    public bool IsProtectedFromBombs => isActiveAndEnabled && Time.time < bombProtectionUntil;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;
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

        playerHealth = GetComponent<PlayerHealth>();
        crateEffects = GetComponent<PlayerCrateEffects>();
        float closest = float.PositiveInfinity;
        for (int i = 0; i < 3; i++)
        {
            float distance = Mathf.Abs(transform.position.x - LaneX(i));
            if (distance < closest)
            {
                closest = distance;
                targetLane = i;
            }
        }

        targetX = LaneX(targetLane);
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead())
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            return;
        }

        ReadLaneInput();
        targetX = LaneX(targetLane);
        MoveToLane();
        UpdateAnimation();
    }

    private void ReadLaneInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            targetLane = 0;
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            targetLane = 1;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            targetLane = 2;
        }

        isChangingLanes = Mathf.Abs(transform.position.x - targetX) > 0.01f;
    }

    private void MoveToLane()
    {
        float currentX = transform.position.x;
        float multiplier = crateEffects != null ? crateEffects.SpeedMultiplier : 1f;
        float nextX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * multiplier * Time.deltaTime);
        float deltaX = nextX - currentX;
        if (characterController != null)
        {
            characterController.Move(new Vector3(deltaX, 0f, 0f));
        }
        else
        {
            transform.position += new Vector3(deltaX, 0f, 0f);
        }

        if (Mathf.Abs(transform.position.x - currentX) > 0.0001f)
        {
            bombProtectionUntil = Time.time + Mathf.Max(Time.deltaTime, bombProtectionAfterArrival);
        }

        isChangingLanes = Mathf.Abs(transform.position.x - targetX) > 0.01f;
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", isChangingLanes);
        }
    }
}
