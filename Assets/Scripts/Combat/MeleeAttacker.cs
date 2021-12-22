using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeleeAttacker : MonoBehaviour
{

    public float attackDamage = 1f;
    public float attackRate = 0.1f;

    private float lastAttackTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        lastAttackTime = attackRate;
    }

    public void Attack(GameObject victim)
    {
        if (Time.time - lastAttackTime > attackRate)
        {
            lastAttackTime = Time.time;
            return;
        }
        
        TargetController targetController;
        victim.TryGetComponent<TargetController>(out targetController);
        if (targetController != null)
        {
            targetController.TakeDamage(attackDamage);
        }
        lastAttackTime = Time.time;
    }
}
