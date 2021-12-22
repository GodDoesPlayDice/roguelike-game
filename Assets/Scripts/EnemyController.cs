using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent navMeshAgent;
    private TargetController targetController;

    private bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();

        gameObject.TryGetComponent<TargetController>(out targetController);
    }

    void Update()
    {
        if (isDead) return;
        // check if enemy is dead
        if (targetController)
        {
            isDead = targetController.isDead;
            if (isDead)
            {
                EnemyDeath();
                return;
            }
        }

        navMeshAgent.destination = player.transform.position;
    }


    private void EnemyDeath()
    {
        Debug.Log("Enemy" + gameObject.GetInstanceID() + " is Dead");
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        transform.position = transform.position + Vector3.down / 2;
    }
}
