using System;
using System.Collections;
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
        public IWeapon CurrentWeapon;
        public bool autoAttack = false;
        public float autoAttackRate = 0.5f;

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

        private void Start()
        {
            CurrentWeapon = _shootingWeaponController;
            StartCoroutine(AutoAttackCoroutine());
        }

        public void ChangeWeapon()
        {
            if ((MeleeWeapon) CurrentWeapon == _meleeWeaponController)
            {
                CurrentWeapon = _shootingWeaponController;
            }
            else
            {
                CurrentWeapon = _meleeWeaponController;
            }
        }


        private IEnumerator AutoAttackCoroutine()
        {
            for (;;)
            {
                if (_actorController.nearFoeActors.Any() && autoAttack)
                {
                    CurrentWeapon.Attack(_actorController.nearFoeActors.First());
                }

                yield return new WaitForSeconds(autoAttackRate);
            }
        }
    }
}