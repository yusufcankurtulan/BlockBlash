using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int x, y; // Grid'deki 
    public string blockColor; // Renk bilgisi

    private GameManager gameManager;

    private void Start()
    {
        // GameManager'ı sahnede bul
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnMouseDown()
    {
        // Tıklanınca blok grubunu yok et
        List<Block> matchingBlocks = FindMatchingBlocks();

        if (matchingBlocks.Count >= 2) // En az 2 blok eşleşmeli
        {
            DestroyMatchingBlocks(matchingBlocks); // Blokları yok et
            gameManager.StartCoroutine(gameManager.UpdateGrid()); // Grid'i güncelle
        }
    }

    // Aynı renkteki blokları bul
    private List<Block> FindMatchingBlocks()
    {
        List<Block> matchingBlocks = new List<Block>(); // Eşleşen bloklar listesi
        Queue<Block> queue = new Queue<Block>(); // BFS için kuyruk
        HashSet<Block> visited = new HashSet<Block>(); // Ziyaret edilen bloklar

        queue.Enqueue(this); // Başlangıç bloğunu sıraya ekle

        while (queue.Count > 0)
        {
            Block current = queue.Dequeue();

            if (!visited.Contains(current))
            {
                visited.Add(current);
                matchingBlocks.Add(current);

                // Komşuları bul ve sıraya ekle
                foreach (Block neighbor in gameManager.GetNeighbors(current))
                {
                    if (neighbor != null && neighbor.blockColor == this.blockColor && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return matchingBlocks;
    }

    // Blok grubunu yok et
    private void DestroyMatchingBlocks(List<Block> matchingBlocks)
    {
        foreach (Block block in matchingBlocks)
        {
            gameManager.grid[block.x, block.y] = null; // Grid'den kaldır
            Destroy(block.gameObject); // Sahneden sil
        }
    }
}
