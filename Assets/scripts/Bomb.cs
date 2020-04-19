using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb : MonoBehaviour
{
    public void OnMouseDown()
    {
        MovesController moves = GameObject.Find("moves").GetComponent<MovesController>();
        moves.reduceMovesByOne();
        
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getBombedBrick(gameObject.transform);
    }
}
