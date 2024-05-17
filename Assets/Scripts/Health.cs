using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int Lives = 5;
    public bool MainCharacter = false;
    private BombDropper _bombDropper;
    
    private float _invincibilityTimer;
    private float _invincibilityOpacity;
    private bool _invincibilityOpacityGoingUp;
    private bool _isInvincible;
    private const float MinInvincibilityOpacity = 0.2f;
    private const float MaxInvincibilityOpacity = 1.0f;
    private SpriteRenderer _spriteRenderer;

    public static Health PlayerHealth;

    private void Awake()
    {
        _bombDropper = GetComponent<BombDropper>();
        _invincibilityTimer = Time.time;
        _invincibilityOpacity = MaxInvincibilityOpacity;
        _invincibilityOpacityGoingUp = false;
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _isInvincible = false;
        if (MainCharacter)
        {
            PlayerHealth = this;
        }
    }

    public void Damage()
    {
        if (this._invincibilityTimer > Time.time)
        {
            return;
        }
        this._isInvincible = true;
        this._invincibilityTimer = Time.time + 2.0f;
        this.Lives--;
        if (MainCharacter)
        {
            UIController.Instance.ShownLives = Lives;
            if (Lives <= 0)
            {
                UIController.Instance.EndGame(false);
            }
        }

        if (Lives <= 0)
        {
            GameController.Remove(_bombDropper);
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (this._invincibilityTimer > Time.time)
        {
            float invincibilityOpacityStep = Time.deltaTime * 6 * (MaxInvincibilityOpacity - MinInvincibilityOpacity);
            if (_invincibilityOpacityGoingUp)
            {
                this._invincibilityOpacity = Mathf.Clamp(this._invincibilityOpacity + invincibilityOpacityStep,
                    MinInvincibilityOpacity, MaxInvincibilityOpacity);
            }
            else
            {
                this._invincibilityOpacity = Mathf.Clamp(this._invincibilityOpacity - invincibilityOpacityStep,
                    MinInvincibilityOpacity, MaxInvincibilityOpacity);
            }

            if (this._invincibilityOpacity >= MaxInvincibilityOpacity)
            {
                this._invincibilityOpacityGoingUp = false;
            } else if (this._invincibilityOpacity <= MinInvincibilityOpacity)
            {
                this._invincibilityOpacityGoingUp = true;
            }
        }
        else
        {
            _isInvincible = false;
            this._invincibilityOpacity = MaxInvincibilityOpacity;
        }
        Color currColor = this._spriteRenderer.color;
        this._spriteRenderer.color = new Color(currColor.r, currColor.g, currColor.b, this._invincibilityOpacity);
    }
}
