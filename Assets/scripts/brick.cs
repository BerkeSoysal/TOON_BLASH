using System;
using System.Collections.Generic;
using UnityEngine;

public class brick : MonoBehaviour
{
    private int x = -1;

    private int y = -1;

    private Dictionary<int, int> _dictionary;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    

    private void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getClickedBrick(gameObject.transform);
    }
}
