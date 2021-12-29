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

    // enemies part
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemy;
        public int amount;
    }
    public EnemyType[] enemiesToSpawn;
    public int minEnemiesToStartSpawn = 1;
    public int singleSpawnPortion = 3;

    private int _killsToWinCount = 0;
    private int _currentEnemiesInRoom = 0;
    private int _pointerToNextEnemyToSpawn = 0;

    // objects to find 
    private DoorController[] doors;
    private EnemySpawner[] enemySpawners;

    void Start()
    {
        state = RoomState.untouched;
        doors = GetDoors();
        enemySpawners = GetEnemySpawners();
        _killsToWinCount = Get_killsToWinCount();
    }

    void Update()
    {
        switch (state)
        {
            case RoomState.untouched:
                break;
            case RoomState.battle:
                // spawn enemies
                if (_currentEnemiesInRoom < minEnemiesToStartSpawn)
                {
                    SpawnEnemiesPortion();
                }
                // check if there are alive enemies
                if (_killsToWinCount <= 0)
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
        _killsToWinCount--;
        _currentEnemiesInRoom--;

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

    private EnemySpawner[] GetEnemySpawners()
    {
        // all spawners in this scene
        EnemySpawner[] enemySpawners = GameObject.FindObjectsOfType<EnemySpawner>();

        // filter doors to get ones connected to this room
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        EnemySpawner[] result = enemySpawners.Where<EnemySpawner>(enemySpawner =>
        boxCollider.bounds.Contains(enemySpawner.gameObject.transform.position)).ToArray();

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

    private void SpawnEnemiesPortion()
    {
        if (_killsToWinCount > 0 && enemySpawners.Length > 0)
        {
            int mainIndex = 0;
            // standart spawn portion or all remaining enemies
            int needToSpawnCount = _killsToWinCount - singleSpawnPortion <= 0 ? _killsToWinCount : singleSpawnPortion;

            foreach (EnemyType enemy in enemiesToSpawn)
            {
                for (int i = 0; i < enemy.amount; i++)
                {
                    if (mainIndex == _pointerToNextEnemyToSpawn && needToSpawnCount > 0)
                    {
                        SpawnSingleEnemy(enemy.enemy);
                        // quantity control
                        needToSpawnCount--;
                        // control of enemy selection
                        _pointerToNextEnemyToSpawn++;
                    }
                    // control of enemy selection
                    mainIndex++;
                }
            }
        }
    }

    private void SpawnSingleEnemy(GameObject enemy)
    {
        EnemySpawner randomSpawner = enemySpawners[UnityEngine.Random.Range(0, enemySpawners.Length - 1)];
        GameObject spawnedEnemy = randomSpawner.SpawnEnemy(enemy);
        // subscribe to death event
        TargetController enemyTarget;
        spawnedEnemy.TryGetComponent<TargetController>(out enemyTarget);
        if (enemyTarget != null)
        {
            enemyTarget.events.onDeathEvent.AddListener(this.OnEnemyDeath);
            _currentEnemiesInRoom++;
        }
    }

    private int Get_killsToWinCount()
    {
        int result = 0;
        if (enemySpawners.Length > 0)
        {
            // calculating kills to win
            foreach (EnemyType enemy in enemiesToSpawn)
            {
                for (int i = 0; i < enemy.amount; i++)
                {
                    result++;
                }
            }
        }

        return result;
    }


}
