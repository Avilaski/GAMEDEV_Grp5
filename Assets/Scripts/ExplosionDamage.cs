using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ExplosionDamage : MonoBehaviour
{
    private BoxCollider hitbox;
    private int playerDamage;
    private LayerMask targetLayers;
    private bool initialized;
    private readonly HashSet<PlayerHealth> hitPlayers = new HashSet<PlayerHealth>();
    private readonly HashSet<LaneMover> hitObjects = new HashSet<LaneMover>();

    public void Initialize(int damage, LayerMask layers)
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
        playerDamage = Mathf.Max(0, damage);
        targetLayers = layers;
        FitHeightToFlame();
        initialized = true;
    }

    private void FitHeightToFlame()
    {
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            MeshFilter mesh = renderer.GetComponent<MeshFilter>();
            if (!renderer.enabled || mesh == null || mesh.sharedMesh == null)
            {
                continue;
            }

            Bounds bounds = mesh.sharedMesh.bounds;
            Matrix4x4 toLocal = transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 point = bounds.center + Vector3.Scale(bounds.extents, new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f));
                float y = toLocal.MultiplyPoint3x4(point).y;
                bottom = Mathf.Min(bottom, y);
                top = Mathf.Max(top, y);
            }
        }

        if (float.IsInfinity(bottom))
        {
            return;
        }

        Vector3 size = hitbox.size;
        size.y = Mathf.Max(0.01f, top - bottom);
        hitbox.size = size;
        Vector3 center = hitbox.center;
        center.y = (top + bottom) * 0.5f;
        hitbox.center = center;
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            return;
        }

        Vector3 scale = transform.lossyScale;
        Vector3 halfSize = Vector3.Scale(hitbox.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * 0.5f;
        if (Mathf.Min(halfSize.x, halfSize.y, halfSize.z) < 0.001f)
        {
            return;
        }

        Physics.SyncTransforms();
        Collider[] overlaps = Physics.OverlapBox(transform.TransformPoint(hitbox.center), halfSize, transform.rotation, targetLayers, QueryTriggerInteraction.Collide);
        foreach (Collider other in overlaps)
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                PlayerLaneMovement movement = health.GetComponent<PlayerLaneMovement>();
                if (movement != null && movement.IsProtectedFromBombs)
                {
                    continue;
                }

                if (hitPlayers.Add(health))
                {
                    health.TakeDamage(playerDamage);
                }

                continue;
            }

            LaneMover target = other.GetComponentInParent<LaneMover>();
            if (target != null && hitObjects.Add(target))
            {
                target.DestroyByBlast();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(box.center, box.size);
        Gizmos.matrix = previous;
    }
}
