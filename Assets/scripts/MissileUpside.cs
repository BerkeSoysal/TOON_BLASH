using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileUpside : MonoBehaviour
{
    public void OnMouseDown()
    {
        movesController moves = GameObject.Find("moves").GetComponent<movesController>();
        moves.reduceMovesByOne();
        
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getMissiledBrickUpside(gameObject.transform);
    }
}