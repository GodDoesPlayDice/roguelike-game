using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Combat.Weapon;
using Enemies;
using Unity.VisualScripting;
using UnityEngine;

namespace Combat
{
    public class ActorCombatController : MonoBehaviour
    {
        public float meleeAttackRadius = 2f;
        public float shootAttackRadius = 5f;

        private MeleeWeapon _meleeWeaponController;
        private ShootingWeapon _shootingWeaponController;

        private void Awake()
        {
            TryGetComponent(out _meleeWeaponController);
            TryGetComponent(out _shootingWeaponController);
        }

        public void ChangeWeapon()
        {
        }

        public void MeleeAttack()
        {
            if (_meleeWeaponController == null) return;
            Collider[] enemies = Physics.OverlapSphere(transform.position, meleeAttackRadius);

            foreach (Collider enemy in enemies)
            {
                _meleeWeaponController.Attack(enemy.gameObject);
            }
        }

        public void ShootAttack()
        {
            if (_shootingWeaponController == null) return;

            // TODO: refactor this. need more optimized way 
            var actors = FindObjectsOfType<MonoBehaviour>().OfType<IActor>()
                .Select(t => t.thisObject)
                .OrderBy(o => Vector3.Distance(gameObject.transform.position, o.transform.position));
            _shootingWeaponController.Attack(actors.First(o =>
            {
                var targetController = o.GetComponent<TargetController>();
                return !CompareTag(o.tag) && !targetController.isDead;
            }));
        }
    }
}