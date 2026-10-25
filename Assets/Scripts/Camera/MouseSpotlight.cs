using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// CameraPOV moves the same camera between views, so this ray automatically
// follows all perspectives. Only the light's child transform rotates.
[RequireComponent(typeof(Light))]
public class MouseSpotlight : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform ignoreRoot;
    [SerializeField] private LayerMask aimLayers = Physics.DefaultRaycastLayers;
    [SerializeField, Min(1f)] private float maxAimDistance = 100f;

    private readonly RaycastHit[] hits = new RaycastHit[64];

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
        if (GetComponent<Light>().type != LightType.Spot)
        {
            Debug.LogWarning("MouseSpotlight needs a Spot Light.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (Mouse.current == null || aimCamera == null || !aimCamera.isActiveAndEnabled)
            return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 cursor = Mouse.current.position.ReadValue();
        if (!aimCamera.pixelRect.Contains(cursor))
            return;

        Ray ray = aimCamera.ScreenPointToRay(cursor);
        Physics.SyncTransforms();
        int count = Physics.RaycastNonAlloc(ray, hits, maxAimDistance, aimLayers,
            QueryTriggerInteraction.Collide);
        RaycastHit[] candidates = hits;
        // NonAlloc does not guarantee which hits are returned when the buffer fills.
        if (count == hits.Length)
        {
            candidates = Physics.RaycastAll(ray, maxAimDistance, aimLayers, QueryTriggerInteraction.Collide);
            count = candidates.Length;
        }

        float closestDistance = float.PositiveInfinity;
        Vector3 target = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            Collider hit = candidates[i].collider;
            if (hit == null || hit.transform.IsChildOf(transform) ||
                (ignoreRoot != null && hit.transform.IsChildOf(ignoreRoot)))
                continue;

            // Aim through moving lane objects and players at the scenery beneath.
            // Check ancestors too, since models can have colliders on children.
            if (hit.isTrigger || hit.GetComponentInParent<LaneMover>() != null ||
                hit.GetComponentInParent<LaneEnemy>() != null ||
                hit.GetComponentInParent<PlayerHealth>() != null ||
                hit.GetComponentInParent<PlayerController>() != null)
                continue;
            if (candidates[i].distance >= closestDistance)
                continue;
            closestDistance = candidates[i].distance;
            target = candidates[i].point;
        }
        if (float.IsPositiveInfinity(closestDistance))
            return;

        Vector3 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
    }
}
