﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class brick : MonoBehaviour
{    
    private void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.GetClickedBrick(gameObject.transform);
    }
}
