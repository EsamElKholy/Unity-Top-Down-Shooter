using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform mapFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range (0, 1)]
    public float outlinePercent;    

    public float tileSize;

    List<Coord> tileCoords;
    Queue<Coord> shuffledTiles;
    Queue<Coord> shuffledOpenTiles;

    Transform[,] tileMap;

    Map currentMap;

    // Use this for initialization
	void Start ()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    // Update is called once per frame
    void Update ()
    {
	}

    void OnNewWave(int waveNum)
    {
        mapIndex = waveNum - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (maps != null)
        {
            if (maps.Length > 0 && mapIndex < maps.Length && mapIndex >= 0)
            {
                currentMap = maps[mapIndex];

                System.Random prng = new System.Random(currentMap.seed);

                tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

                // Generating tile coords
                tileCoords = new List<Coord>();

                for (int i = 0; i < currentMap.mapSize.x; i++)
                {
                    for (int j = 0; j < currentMap.mapSize.y; j++)
                    {
                        tileCoords.Add(new Coord(i, j));
                    }
                }

                shuffledTiles = new Queue<Coord>(Utility.ShuffleArray(tileCoords.ToArray(), currentMap.seed));

                // Generating map holder
                string mapHolderName = "Generated Map";

                if (transform.FindChild(mapHolderName))
                {
                    DestroyImmediate(transform.FindChild(mapHolderName).gameObject);
                }

                Transform mapHolder = new GameObject(mapHolderName).transform;
                mapHolder.SetParent(transform);

                // Spawning tiles
                for (int i = 0; i < currentMap.mapSize.x; i++)
                {
                    for (int j = 0; j < currentMap.mapSize.y; j++)
                    {
                        Vector3 tilePos = CoordToPosition(i, j);

                        Transform newTile = Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90.0f)) as Transform;
                        newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                        newTile.SetParent(mapHolder);
                        tileMap[i, j] = newTile;
                    }
                }

                // Spawning obstacles
                bool[,] obstacleMap = new bool[currentMap.mapSize.x, currentMap.mapSize.y];

                List<Coord> allOpenCoords = new List<Coord>(tileCoords);

                int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercentage);
                int currentObstacleCount = 0;

                for (int i = 0; i < obstacleCount; i++)
                {
                    Coord rc = GetRandomCoord();

                    obstacleMap[rc.x, rc.y] = true;
                    currentObstacleCount++;

                    if (rc != currentMap.mapCenter && MapIsFullyAccessable(obstacleMap, currentObstacleCount))
                    {
                        float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                        Vector3 obstaclePos = CoordToPosition(rc.x, rc.y);

                        Transform newObstacle = Instantiate(obstaclePrefab, obstaclePos + Vector3.up * obstacleHeight / 2.0f, Quaternion.identity) as Transform;
                        newObstacle.SetParent(mapHolder);
                        newObstacle.localScale = new Vector3(((1 - outlinePercent) * tileSize), obstacleHeight, ((1 - outlinePercent) * tileSize));

                        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                        float colorPercent = rc.y / (float)currentMap.mapSize.y;

                        Color obstacleColor = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                        obstacleMaterial.color = obstacleColor;

                        obstacleRenderer.sharedMaterial = obstacleMaterial;

                        allOpenCoords.Remove(rc);
                    }
                    else
                    {
                        currentObstacleCount--;
                        obstacleMap[rc.x, rc.y] = false;
                    }
                }

                shuffledOpenTiles = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

                // Spawning navigation mesh obstacles
                Transform leftsideMask = Instantiate(navmeshMaskPrefab, Vector3.left * ((currentMap.mapSize.x + maxMapSize.x) / 4.0f) * tileSize, Quaternion.identity) as Transform;
                leftsideMask.SetParent(mapHolder);
                leftsideMask.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2.0f, 1, currentMap.mapSize.y) * tileSize;

                Transform rightsideMask = Instantiate(navmeshMaskPrefab, Vector3.right * ((currentMap.mapSize.x + maxMapSize.x) / 4.0f) * tileSize, Quaternion.identity) as Transform;
                rightsideMask.SetParent(mapHolder);
                rightsideMask.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2.0f, 1, currentMap.mapSize.y) * tileSize;

                Transform topsideMask = Instantiate(navmeshMaskPrefab, Vector3.forward * ((currentMap.mapSize.y + maxMapSize.y) / 4.0f) * tileSize, Quaternion.identity) as Transform;
                topsideMask.SetParent(mapHolder);
                topsideMask.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2.0f) * tileSize;

                Transform bottomsideMask = Instantiate(navmeshMaskPrefab, Vector3.back * ((currentMap.mapSize.y + maxMapSize.y) / 4.0f) * tileSize, Quaternion.identity) as Transform;
                bottomsideMask.SetParent(mapHolder);
                bottomsideMask.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2.0f) * tileSize;

                navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
                mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
            }
        } 
    }

    bool MapIsFullyAccessable(bool[,] map, int obtstacleCount)
    {
        bool[,] mapFlags = new bool[map.GetLength(0), map.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessableTilesCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < map.GetLength(0) && neighbourY >= 0 && neighbourY < map.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !map[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessableTilesCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessableCount = (currentMap.mapSize.x * currentMap.mapSize.y) - obtstacleCount;
                

        return targetAccessableCount == accessableTilesCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3((-currentMap.mapSize.x / 2.0f) + 0.5f + x, 0, (-currentMap.mapSize.y / 2.0f) + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(((currentMap.mapSize.x - 1) / 2.0f) + position.x / tileSize);
        int y = Mathf.RoundToInt(((currentMap.mapSize.y - 1) / 2.0f) + position.z / tileSize);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord c = shuffledTiles.Dequeue();
        shuffledTiles.Enqueue(c);

        return c;
    }

    public Transform GetRandomOpenTile()
    {
        Coord c = shuffledOpenTiles.Dequeue();
        shuffledOpenTiles.Enqueue(c);

        return tileMap[c.x, c.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator==(Coord first, Coord other)
        {
            return (first.x == other.x && first.y == other.y);
        }

        public static bool operator!=(Coord first, Coord other)
        {
            return !(first == other);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;

        [Range(0, 1)]
        public float obstaclePercentage;

        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
