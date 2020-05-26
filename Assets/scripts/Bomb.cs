using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bomb : MonoBehaviour
{
    public void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getBombedBrick(gameObject.transform);
    }
}
