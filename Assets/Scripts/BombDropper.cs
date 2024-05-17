using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDropper : MonoBehaviour
{
    public TeamData TeamData;
    private Bomb _bombPreFab;
    public int LaserRange = 3;
    void Start()
    {
        _bombPreFab = Resources.Load("Prefabs/Bomb", typeof(Bomb)) as Bomb;
        gameObject.GetComponent<SpriteRenderer>().color = TeamData.TeamColor;
    }

    public bool DropBomb()
    {
        foreach (Collider2D coll in Physics2D.OverlapPointAll(GameGrid.CenteredInCell(transform.position)))
        {
            if (coll.GetComponent<Bomb>() != null)
                return false;
        }
        Bomb.Create(_bombPreFab, this);
        
        return true;
    }
}
