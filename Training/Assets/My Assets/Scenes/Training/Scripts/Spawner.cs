using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;
    public Wave[] enemyWaves;
    public Enemy enemy;

    Wave currentWave;
    int currentWaveIndex;

    int remainingEnemyCount;
    int enemiesAlive;
    float nextSpawnTime;

    MapGenerator map;

    LivingEntity playerEntity;
    Transform playerT;

    float timeBetweenCamps = 1.0f;
    float distanceBetweenCamps = 1.0f;
    float nextCampTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

	// Use this for initialization
	void Start ()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        playerEntity.OnDeath += OnPlayerDeath;

        nextCampTime = Time.time + timeBetweenCamps;
        campPositionOld = playerT.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampTime)
            {
                nextCampTime = Time.time + timeBetweenCamps;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < distanceBetweenCamps);
                campPositionOld = playerT.position;
            }

            if ((remainingEnemyCount > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                remainingEnemyCount--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");

                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    Destroy(enemy.gameObject);
                }

                NextWave();
            }
        }
	}

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();

        if (isCamping)
        {
            randomTile = map.GetTileFromPosition(playerT.position);
        }

        Color or = new Color(1, 251 / 255f, 255 / 255f, 1);
        Material tileMat = randomTile.GetComponent<Renderer>().material;

        //if (tileMat.color != Color.red)
        {
            Color originalCol = or;
            Color spawnCol = Color.red;

            float spawnTimer = 0;

            while (spawnTimer < spawnDelay)
            {
                tileMat.color = Color.Lerp(originalCol, spawnCol, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

                spawnTimer += Time.deltaTime;

                yield return null;
            }
        }        

        Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;

        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.enemyColor);
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;

        if (enemiesAlive == 0)
        {
            NextWave();
        }
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    void NextWave()
    {
        if (currentWaveIndex > 0)
        {
            AudioManager.instance.PlaySound2D("level complete");
        }

        currentWaveIndex++;

        if (currentWaveIndex - 1 < enemyWaves.Length)
        {
            currentWave = enemyWaves[currentWaveIndex - 1];
            remainingEnemyCount = currentWave.enemyCount;
            enemiesAlive = currentWave.enemyCount;

            if (OnNewWave != null)
            {
                OnNewWave(currentWaveIndex);
            }

            ResetPlayerPosition();
        }        
    }

    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color enemyColor;
        public bool infinite;
    }
}
