using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPart : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.Damage();
        }
    }
}
