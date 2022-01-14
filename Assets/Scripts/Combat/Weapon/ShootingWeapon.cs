using System;
using System.Collections;
using UnityEngine;

namespace Combat
{
    public class ShootingWeapon : MonoBehaviour, IWeapon
    {
        public GameObject projectile;
        public float damage = 10f;
        public float projectileSpeed = 10f;
        public void Attack(GameObject victim)
        {
            if (projectile == null)
            {
                Debug.Log($"No projectile to shoot {victim} victim", this);
                return;
            }
            Vector3 direction = victim.transform.position - gameObject.transform.position;
            Instantiate(projectile, transform.position, transform.rotation);
            // rigidbody part
            projectile.TryGetComponent<Rigidbody>(out var rb);
            rb ??= projectile.AddComponent<Rigidbody>();
            // collider part
            projectile.TryGetComponent<Collider>(out var projectileCollider);
            projectileCollider ??= projectile.AddComponent<SphereCollider>();
            
            // projectile controller part
            projectile.TryGetComponent<ProjectileController>(out var projectileController);
            projectileController ??= projectile.AddComponent<ProjectileController>();
            projectileController.StartLiveTime();
            
            projectileCollider.isTrigger = true;
            rb.velocity = direction * projectileSpeed;
        }

        

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            
        }
#endif
    }
}