using UnityEngine;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour, IWeapon
    {
        public float damage = 10f;
        public void Attack(GameObject victim)
        {
            victim.TryGetComponent<TargetController>(out var targetController);
            if (targetController != null && targetController.gameObject != gameObject)
            {
                targetController.TakeDamage(damage);
            }
        }
    }
}
