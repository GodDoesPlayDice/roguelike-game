using System;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class ActorCombatController : MonoBehaviour
    {
        public float meleeAttackRadius = 2;

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
        }
    }
}