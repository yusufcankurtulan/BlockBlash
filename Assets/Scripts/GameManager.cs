using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int gridWidth = 7;
    public int gridHeight = 10;
    public float blockSize = 1.1f;
    public float blockFallTime = 1.0f; // Blokların düşüş süresi
    public GameObject[] blockPrefabs;
    public float spawnHeight = 15f; // Blokların spawn olacak yükseklik

    [HideInInspector]
    public GameObject[,] grid;

    private bool isProcessing = false;

    void Start()
    {
        GameObject[] coords = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in coords)
        {
            obj.hideFlags = HideFlags.HideInHierarchy;
            Debug.Log(obj.name + " hiyerarşide gizlendi.");
        }
        grid = new GameObject[gridWidth, gridHeight];
        GenerateGrid();
    }

    public bool CanMakeMove()
    {
        return !isProcessing;
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                AddBlockToGrid(x, y);
            }
        }
    }

    void AddBlockToGrid(int x, int y)
    {
        // Rastgele bir prefab seç
        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject block = Instantiate(blockPrefabs[randomIndex], new Vector3(x * blockSize, spawnHeight, 0), Quaternion.identity); // Yüksekten başlatıyoruz
        block.transform.parent = this.transform;

        // Block script'ine x ve y değerlerini ata
        Block blockComponent = block.GetComponent<Block>();
        blockComponent.x = x;
        blockComponent.y = y;

        // İsmini koordinata göre ayarla
        block.name = $"Block ({x},{y})";

        grid[x, y] = block;

        // Yeni blokları yukarıdan kaydırmak için Coroutine çağır
        StartCoroutine(MoveBlockToGridPosition(block, x, y));
    }

    // Blokları hedef pozisyona kaydıran Coroutine
    IEnumerator MoveBlockToGridPosition(GameObject block, int x, int y)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = block.transform.position;
        Vector3 targetPosition = new Vector3(x * blockSize, y * blockSize, 0);

        // Kayma animasyonunu başlatıyoruz
        while (elapsedTime < blockFallTime)
        {
            if (block == null) yield break; // Blok yok olmuşsa coroutine'i sonlandır

            block.transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / blockFallTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Son pozisyona ayarla
        if (block != null) block.transform.position = targetPosition;
    }

    public List<Block> GetNeighbors(Block block)
    {
        List<Block> neighbors = new List<Block>();
        int[,] directions = new int[,]
        {
            { 0, 1 },  // Yukarı
            { 1, 0 },  // Sağ
            { 0, -1 }, // Aşağı
            { -1, 0 }  // Sol
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newX = block.x + directions[i, 0];
            int newY = block.y + directions[i, 1];

            if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
            {
                GameObject neighborObject = grid[newX, newY];
                if (neighborObject != null)
                {
                    neighbors.Add(neighborObject.GetComponent<Block>());
                }
            }
        }

        return neighbors;
    }

    void DropBlocks()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 1; y < gridHeight; y++)
            {
                if (grid[x, y] != null && grid[x, y - 1] == null)
                {
                    int emptyRow = y - 1;

                    // Boş olan en alt satırı bul
                    while (emptyRow > 0 && grid[x, emptyRow - 1] == null)
                    {
                        emptyRow--;
                    }

                    // Blokları yeni yerine taşı
                    grid[x, emptyRow] = grid[x, y];
                    grid[x, y] = null;

                    // Yeni pozisyona animasyonla taşı
                    StartCoroutine(MoveBlockToGridPosition(grid[x, emptyRow], x, emptyRow));

                    // Blok koordinatlarını güncelle
                    Block block = grid[x, emptyRow].GetComponent<Block>();
                    block.x = x;
                    block.y = emptyRow;

                    // İsmi güncelle
                    block.gameObject.name = $"Block ({x},{emptyRow})";
                }
            }
        }
    }


    void RefillGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    AddBlockToGrid(x, y);
                }
            }
        }
    }
    public IEnumerator UpdateGrid()
    {
        isProcessing = true; // İşlem başlatıldı

        DropBlocks();
        yield return new WaitForSeconds(blockFallTime);

        RefillGrid();
        yield return new WaitForSeconds(blockFallTime);

        isProcessing = false; // İşlem tamamlandı
    }

}
