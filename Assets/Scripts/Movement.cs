using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float Speed = 1.0f;

    public void Move(Vector2 input)
    {
        transform.position += (Vector3) input * (Speed * Time.deltaTime);
    }
    
    public void MoveTowards(Vector2 input)
    {
        Vector2 addVector = Vector2.MoveTowards(transform.position, input, (Speed * Time.deltaTime));
        transform.position = new Vector3(addVector.x, addVector.y, 0);
    }
    public void Move(Vector2 input, Rigidbody2D rb)
    {
        if (rb.velocity.magnitude < Speed)
            rb.AddForce(input);
    }
}
