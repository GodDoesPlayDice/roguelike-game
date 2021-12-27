using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Target;

public class RoomController : MonoBehaviour
{
    public enum RoomState
    {
        untouched,
        battle,
        win
    }
    public RoomState state;

    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemy;
        public int amount;
    }
    public EnemyType[] enemiesToSpawn;

    private DoorController[] doors;
    private int aliveEnemiesCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        state = RoomState.untouched;
        doors = GetDoors();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case RoomState.untouched:
                break;
            case RoomState.battle:
                // check if there are alive enemies
                if (aliveEnemiesCount <= 0)
                {
                    Win();
                }
                break;
            case RoomState.win:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (state == RoomState.untouched)
            {
                StartBattle();
            }
        }
    }

    private void StartBattle()
    {
        state = RoomState.battle;
        // spawn enemies
        SpawnEnemies();
        ToggleDoors("lock");
    }
    private void Win()
    {
        state = RoomState.win;
        Debug.Log("Win in room  " + gameObject.name);
        ToggleDoors("unlock");
    }

    private void OnEnemyDeath(TargetController.OnDeathEventArgs onDeathEventArgs)
    {
        aliveEnemiesCount--;


        // unsubscribe to death event
        TargetController targetController;
        onDeathEventArgs.gameObject.TryGetComponent<TargetController>(out targetController);
        if (targetController != null)
        {
            targetController.events.onDeathEvent.RemoveListener(this.OnEnemyDeath);
        }
    }

    private DoorController[] GetDoors()
    {
        // all doors in this scene
        DoorController[] doorControllers = GameObject.FindObjectsOfType<DoorController>();

        // filter doors to get ones connected to this room
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        DoorController[] result = doorControllers.Where<DoorController>(doorController =>
        (boxCollider.bounds.Contains(doorController.leftSidePoint) ||
        boxCollider.bounds.Contains(doorController.rightSidePoint))).ToArray();

        return result;
    }

    private void ToggleDoors(string action)
    {
        foreach (DoorController door in doors)
        {
            if (action == "lock")
            {
                door.LockDoor();
            }
            else if (action == "unlock")
            {
                door.UnlockDoor();
            }
        }
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn.Length > 0)
        {
            foreach (EnemyType enemy in enemiesToSpawn)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject spawnedEnemy = Instantiate(enemy.enemy, transform.position, transform.rotation);

                    // subscribe to death event
                    TargetController enemyTarget;
                    spawnedEnemy.TryGetComponent<TargetController>(out enemyTarget);
                    if (enemyTarget != null)
                    {
                        enemyTarget.events.onDeathEvent.AddListener(this.OnEnemyDeath);
                        aliveEnemiesCount++;
                    }
                }
            }
        }
    }
}
