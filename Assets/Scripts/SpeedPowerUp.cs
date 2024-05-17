using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    public float IncreaseAmount = 0.4f;
    
    private void Start()
    {
        Vector2Int gameBlockI = GameGrid.Instance.IndexesFromWorldPos(transform.position);
        GameGrid.Instance.gameGrid[gameBlockI.x][gameBlockI.y].HasPowerUp = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Movement mover = other.GetComponent<Movement>();
        if (mover != null)
        {
            mover.Speed += IncreaseAmount;
            Vector2Int gameBlockI = GameGrid.Instance.IndexesFromWorldPos(transform.position);
            GameGrid.Instance.gameGrid[gameBlockI.x][gameBlockI.y].HasPowerUp = false;
            Destroy(gameObject);
        }
    }
}
