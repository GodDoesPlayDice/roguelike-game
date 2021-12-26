using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Target; 

public class MeleeAttacker : MonoBehaviour
{
    public void Attack(GameObject victim, float damageAmount)
    {    
        TargetController targetController;
        victim.TryGetComponent<TargetController>(out targetController);
        if (targetController != null)
        {
            targetController.TakeDamage(damageAmount);
        }
    }
}
