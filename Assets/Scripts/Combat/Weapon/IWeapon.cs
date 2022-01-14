using UnityEngine;

namespace Combat
{
    public interface IWeapon
    {
        public void Attack(GameObject victim);
    }
}