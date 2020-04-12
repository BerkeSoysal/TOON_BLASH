using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public void OnMouseDown()
    {
        movesController moves = GameObject.Find("moves").GetComponent<movesController>();
        moves.reduceMovesByOne();
        
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getMissiledBrick(gameObject.transform);
    }
}
