using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;

    private GameObject[] spawnPoints;
    private int spawnIndex;
    private int enemyIndex;

    //Two variables used to show the minimup and maximum time possible between each enemy spawn
    public static float defaultMinSpawnTime;
    public static float defaultMaxSpawnTime;
    public static float minSpawnTime;
    public static float maxSpawnTime;
    private float waitTime;

    public static int enemiesToSpawn;
    public static int enemiesKilled;

    private void Start()
    {
        EnemyBase.StartingValues();

        defaultMinSpawnTime = 1f;
        defaultMaxSpawnTime = 3f;
        minSpawnTime = defaultMinSpawnTime;
        maxSpawnTime = defaultMaxSpawnTime;

        enemiesToSpawn = 0;
        enemiesKilled = 0;
    }

    public IEnumerator Spawn()
    {
        FindSpawnPoints();

        enemiesToSpawn++;
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            waitTime = Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(waitTime);

            spawnIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPos = spawnPoints[spawnIndex].transform.position;
            enemyIndex = Random.Range(0, enemies.Length);
            Instantiate(enemies[enemyIndex], spawnPos, Quaternion.identity);
        }

        ClearSpawnPoints();
    }

    void FindSpawnPoints()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }

    void ClearSpawnPoints()
    {
        foreach (GameObject spawnPoint in spawnPoints)
        {
            Destroy(spawnPoint);
        }
    }
}
