using System;
using System.Collections.Generic;
using UnityEngine;

public class brick : MonoBehaviour
{


    private Dictionary<int, int> _dictionary;
    
    private void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getClickedBrick(gameObject.transform);
    }
}
