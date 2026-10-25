using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BombController))]
public class BombBlast : MonoBehaviour
{
    [SerializeField] private BoxCollider blastZone;
    [SerializeField] private LayerMask targetLayers = ~0;

    private BombController bomb;
    private void Awake()
    {
        bomb = GetComponent<BombController>();
    }

    private void OnEnable()
    {
        bomb.Exploded += Explode;
    }

    private void OnDisable()
    {
        bomb.Exploded -= Explode;
    }

    private void Explode(Vector3 bombPosition)
    {
        if (blastZone == null)
        {
            Debug.LogWarning("Assign a Blast Zone BoxCollider to BombBlast.", this);
            return;
        }

        // LaneMover writes transforms during Update; query their latest positions.
        Physics.SyncTransforms();
        Transform zone = blastZone.transform;
        Vector3 scale = zone.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 halfSize = Vector3.Scale(blastZone.size, scale) * 0.5f;
        Collider[] hits = Physics.OverlapBox(zone.TransformPoint(blastZone.center),
            halfSize, zone.rotation, targetLayers, QueryTriggerInteraction.Collide);

        var targets = new HashSet<LaneMover>();
        int destroyed = 0;
        foreach (Collider hit in hits)
        {
            LaneMover target = hit.GetComponentInParent<LaneMover>();
            if (target != null && targets.Add(target) && target.DestroyByBlast())
                destroyed++;
        }

        Debug.Log($"Blast destroyed {destroyed} lane object(s).", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (blastZone == null)
            return;
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = blastZone.transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(blastZone.center, blastZone.size);
        Gizmos.matrix = previous;
    }
}
