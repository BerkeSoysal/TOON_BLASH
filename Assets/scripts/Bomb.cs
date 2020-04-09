using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb : MonoBehaviour
{ 
    private void OnDestroy()
    {
        
    }

    private void OnMouseDown()
    {
        movesController moves = GameObject.Find("moves").GetComponent<movesController>();
        moves.reduceMovesByOne();
        
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getBombedBrick(gameObject.transform);
    }
}
