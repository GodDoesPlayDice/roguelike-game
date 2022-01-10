using UnityEngine;

namespace Combat
{
    public class MeleeAttacker : MonoBehaviour
    {
        public void Attack(GameObject victim, float damageAmount)
        {
            victim.TryGetComponent<TargetController>(out var targetController);
            if (targetController != null && targetController.gameObject != gameObject)
            {
                targetController.TakeDamage(damageAmount);
            }
        }
    }
}
