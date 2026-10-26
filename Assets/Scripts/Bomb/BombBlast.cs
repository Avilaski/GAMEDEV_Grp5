using UnityEngine;

[RequireComponent(typeof(BombController))]
public class BombBlast : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int playerDamage = 1;
    [SerializeField]
    private LayerMask targetLayers = ~0;
    private BombController bomb;

    private void Awake()
    {
        bomb = GetComponent<BombController>();
    }

    private void OnEnable()
    {
        bomb.ExplosionSpawned += ConfigureDamage;
    }

    private void OnDisable()
    {
        bomb.ExplosionSpawned -= ConfigureDamage;
    }

    private void ConfigureDamage(GameObject explosion)
    {
        ExplosionDamage damage = explosion.GetComponent<ExplosionDamage>();
        if (damage == null)
        {
            Debug.LogWarning("The explosion prefab needs an ExplosionDamage component and BoxCollider", this);
            return;
        }

        damage.Initialize(playerDamage, targetLayers);
    }
}
