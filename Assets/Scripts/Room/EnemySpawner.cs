using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector3 randomPosition;
    public GameObject SpawnEnemy(GameObject enemy)
    {
        Vector3 randomDir = UnityEngine.Random.insideUnitCircle.normalized * Random.Range(1, 2);
        Vector3 randomSpawnPos = transform.position + randomDir;
        GameObject spawnedEnemy = Instantiate(enemy, randomSpawnPos, transform.rotation);
        return spawnedEnemy;
    }
}
