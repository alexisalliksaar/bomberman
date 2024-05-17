using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAligner : MonoBehaviour
{
    public static GridAligner Instance;

    void Awake()
    {
        Instance = this;
    }
    
}
