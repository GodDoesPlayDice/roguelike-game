using UnityEngine;

namespace Combat.Weapon
{
    public class ShootingWeapon : MonoBehaviour, IWeapon
    {
        public GameObject projectile;
        public float damage = 10f;
        public float projectileSpeed = 10f;
        public void Attack(GameObject victim = null)
        {
            if (victim == null) return;
            Debug.Log($"Shot victim: {victim.name}");
            if (projectile == null)
            {
                Debug.Log($"No projectile to shoot {victim} victim", this);
                return;
            }
            var direction = victim.transform.position - gameObject.transform.position;
            var projectileInstance = Instantiate(projectile, transform.position, transform.rotation);
            
            // rigidbody part
            projectileInstance.TryGetComponent<Rigidbody>(out var rb);
            rb ??= projectileInstance.AddComponent<Rigidbody>();
            // collider part
            projectileInstance.TryGetComponent<Collider>(out var projectileCollider);
            projectileCollider ??= projectileInstance.AddComponent<SphereCollider>();
            
            // projectile controller part
            projectileInstance.TryGetComponent<ProjectileController>(out var projectileController);
            projectileController ??= projectileInstance.AddComponent<ProjectileController>();
            projectileController.damage = damage;
            projectileCollider.isTrigger = true;
            rb.velocity = direction.normalized * projectileSpeed;
            projectileController.StartLiveTime();
        }
    }
}