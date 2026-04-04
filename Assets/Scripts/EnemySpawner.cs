using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Danh sách các loại lính/quái
    public float spawnRate = 2f;      // Vài giây ra 1 con
    private float nextSpawnTime;

    public Transform[] spawnPoints;   // Những điểm viền góc màn hình để trào ra

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Bốc đại 1 loại lính và 1 cổng chào
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        int randomPoint = Random.Range(0, spawnPoints.Length);

        // Sinh lính tại cổng đó
        Instantiate(enemyPrefabs[randomEnemy], spawnPoints[randomPoint].position, Quaternion.identity);
    }
}
