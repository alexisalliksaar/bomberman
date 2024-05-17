using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPowerUp : MonoBehaviour
{
    public int IncreaseAmount = 1;

    private void Start()
    {
        Vector2Int gameBlockI = GameGrid.Instance.IndexesFromWorldPos(transform.position);
        GameGrid.Instance.gameGrid[gameBlockI.x][gameBlockI.y].HasPowerUp = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BombDropper dropper = other.GetComponent<BombDropper>();
        if (dropper != null)
        {
            dropper.LaserRange += IncreaseAmount;
            Vector2Int gameBlockI = GameGrid.Instance.IndexesFromWorldPos(transform.position);
            GameGrid.Instance.gameGrid[gameBlockI.x][gameBlockI.y].HasPowerUp = false;
            Destroy(gameObject);
        }
    }
}
