using System;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{    
    private void OnMouseDown()
    {
        MainLogic mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetClickedBrick(gameObject.transform);
    }
}
