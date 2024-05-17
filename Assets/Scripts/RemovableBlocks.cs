using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RemovableBlocks : MonoBehaviour
{
    private Tilemap _tilemap;
    public static RemovableBlocks Instance;
    public List<GameObject> powerUps;
    public float powerUpChance = 0.15f;
    void Start()
    {
        Instance = this;
        _tilemap = gameObject.GetComponent<Tilemap>();
    }

    public void RemoveBlockAt(Vector3 worldPos)
    {
        _tilemap.SetTile(_tilemap.WorldToCell(worldPos), null);
        if (powerUps.Count > 0)
        {
            if (Random.Range(0f, 1f) < powerUpChance)
            {
                int powerUpI = Random.Range(0, powerUps.Count);
                GameObject powerUp = Instantiate(powerUps[powerUpI]);
                powerUp.transform.position = worldPos;
            }
        }
    }
}
