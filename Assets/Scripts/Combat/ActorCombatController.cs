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

        private ActorController _actorController;
        private void Awake()
        {
            TryGetComponent(out _meleeWeaponController);
            TryGetComponent(out _shootingWeaponController);
            TryGetComponent(out _actorController);
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
            if (!_actorController.nearFoeActors.Any()) return;
            var first = _actorController.nearFoeActors.First();
            _shootingWeaponController.Attack(first);
        }
    }
}