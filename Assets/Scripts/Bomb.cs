using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float BlowUpTime = 3.0f;
    public Laser LaserPrefab;
    public float LaserTime = 1.0f;
    public int LaserRange;
    
    private TeamData _teamData;
    private SpriteRenderer _renderer;

    public TeamData TeamData
    {
        get => _teamData;
        set
        {
            _teamData = value;
            _renderer.color = _teamData.TeamColor;
        }
    }

    void Awake()
    {
        _renderer = gameObject.GetComponent<SpriteRenderer>();
        Destroy(gameObject, BlowUpTime);
        GameController.ControllerScripts.Add(this);
    }

    private void OnDestroy()
    {
        GameGrid.BombExplosionTiles explosionTiles = GameGrid.Instance.ExplodeBomb(this);
        foreach (Vector3 tilePos in explosionTiles.RemovedTiles)
        {
            RemovableBlocks.Instance.RemoveBlockAt(tilePos);
        }
        //Debug.Log($"U:{explosionTiles.Top}, D:{explosionTiles.Down}, R:{explosionTiles.Right}, L:{explosionTiles.Left}");
        GameController.ControllerScripts.Remove(this);
        Laser.Create(gameObject, LaserPrefab, LaserTime, _teamData, explosionTiles);
    }

    public static void Create(Bomb prefab, BombDropper creator)
    {
        Bomb bomb = Instantiate(prefab);
        bomb.transform.position = GameGrid.CenteredInCell(creator.transform.position);
        bomb.TeamData = creator.TeamData;
        bomb.LaserRange = creator.LaserRange;
        GameGrid.Instance.PlaceBombDanger(bomb);
    }
}
