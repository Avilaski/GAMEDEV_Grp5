using UnityEngine;

// Plays the prefab's default Animator state once, then removes the instance.
[RequireComponent(typeof(Animator))]
public class ExplosionVfxLifetime : MonoBehaviour
{
    private void Start()
    {
        Animator animator = GetComponent<Animator>();
        animator.Rebind();
        animator.Update(0f);
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float duration = state.length / Mathf.Max(0.001f,
            Mathf.Abs(animator.speed * state.speed * state.speedMultiplier));
        Destroy(gameObject, duration > 0f ? duration : 0.4f);
    }
}
