using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public int Left = 1;
    public int Right = 1;
    public int Top = 1;
    public int Down = 1;
    public TeamData TeamData;
    
    private LaserPart _laserSectionPrefab;
    private Sprite _ending;

    private SpriteRenderer _currRenderer;
    private GameGrid.BombExplosionTiles _bombExplosionTiles;
    
    void Awake()
    {
        _laserSectionPrefab = Resources.Load<LaserPart>("Prefabs/LaserSection");
        _ending = Resources.Load<Sprite>("Laser/laser_top");
        GameController.ControllerScripts.Add(this);
    }

    private void CreateParts()
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = TeamData.SortingLayerOrder * 3 + 1;
        SpriteRenderer childRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        childRenderer.color = TeamData.TeamColor;
        childRenderer.sortingOrder = TeamData.SortingLayerOrder * 3 + 0;
        
        for (int i = 0; i <= Top; i++)
        {
            if (i == Top)
            {
                GenerateEnding(0, Vector3.up * i);
                break;
            }
            GenerateSection(0, Vector3.up * (i + 1));
        }
        
        for (int i = 0; i <= Down; i++)
        {
            if (i == Down)
            {
                GenerateEnding(180, Vector3.down * i);
                break;
            }
            GenerateSection(0, Vector3.down * (i + 1));
        }
        for (int i = 0; i <= Left; i++)
        {
            if (i == Left)
            {
                GenerateEnding(90, Vector3.left * i);
                break;
            }
            GenerateSection(90, Vector3.left * (i + 1));
        }
        for (int i = 0; i <= Right; i++)
        {
            if (i == Right)
            {
                GenerateEnding(-90, Vector3.right * i);
                break;
            }
            GenerateSection(-90, Vector3.right * (i + 1));
        }
    }

    private void GenerateEnding(float zRotation, Vector3 positionChange)
    {
        GameObject edge = DefaultGO();
        SetSprite(_ending, true);
        _currRenderer.sortingOrder = TeamData.SortingLayerOrder * 3 + 2;
        edge.transform.position += positionChange;
        edge.transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }

    private void GenerateSection(float zRotation, Vector3 posChange)
    {
        LaserPart laserSection = Instantiate(_laserSectionPrefab);

        laserSection.transform.parent = gameObject.transform;
        laserSection.transform.localPosition = Vector3.zero;
        laserSection.transform.position += posChange;
        laserSection.transform.rotation = Quaternion.Euler(0, 0, zRotation);
        
        laserSection.gameObject.GetComponent<SpriteRenderer>().sortingOrder = TeamData.SortingLayerOrder * 3 + 1;
        SpriteRenderer childRenderer = laserSection.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        childRenderer.color = TeamData.TeamColor;
        childRenderer.sortingOrder = TeamData.SortingLayerOrder * 3 + 0;
    }

    private void SetSprite(Sprite sprite, bool doColor)
    {
        if (doColor)
        {
            _currRenderer.color = TeamData.TeamColor;
        }
        SetSprite(sprite);
    }
    private void SetSprite(Sprite sprite)
    {
        _currRenderer.sprite = sprite;
    }

    private GameObject DefaultGO()
    {
        GameObject res = new GameObject();
        res.transform.parent = gameObject.transform;
        res.transform.localPosition = Vector3.zero;
        _currRenderer = res.AddComponent<SpriteRenderer>();
        return res;
    }

    public static Laser Create(GameObject parent, Laser prefab, float laserTime, TeamData teamData, GameGrid.BombExplosionTiles bombExplosionTiles)
    {
        Laser res = Instantiate(prefab, parent.transform.position, Quaternion.Euler(0, 0, 0));

        res.TeamData = teamData;
        res._bombExplosionTiles = bombExplosionTiles;
        res.Left = bombExplosionTiles.Left;
        res.Right = bombExplosionTiles.Right;
        res.Top = bombExplosionTiles.Top;
        res.Down = bombExplosionTiles.Down;
        res.CreateParts();
        Destroy(res.gameObject, laserTime);

        return res;
    }

    private void OnDestroy()
    {
        GameGrid.Instance.RemoveBombDanger(this._bombExplosionTiles);
        GameController.ControllerScripts.Remove(this);
    }
}
