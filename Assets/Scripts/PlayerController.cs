using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(BombDropper))]
public class PlayerController : MonoBehaviour
{
    
    private Movement _movement;
    private BombDropper _bombDropper;
    private Rigidbody2D _rb;
    void Start()
    {
        _movement = GetComponent<Movement>();
        _bombDropper = GetComponent<BombDropper>();
        _rb = GetComponent<Rigidbody2D>();
        GameController.Players.Add(_bombDropper);
        GameController.ControllerScripts.Add(this);
    }

    void Update()
    {
        HandleMovement();
        HandleBombDrop();
    }

    private void HandleBombDrop()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _bombDropper.DropBomb();
        }
    }

    /*private void LateUpdate()
    {
        HandleMovement();
    }*/

    private void HandleMovement()
    {
        Vector2 move = Vector2.zero;
        move += Vector2.right * Input.GetAxis("Horizontal");
        move += Vector2.up * Input.GetAxis("Vertical");
        _movement.Move(move);
    }
}
