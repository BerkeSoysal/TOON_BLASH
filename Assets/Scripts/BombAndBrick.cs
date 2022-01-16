using System.Collections.Generic;
using UnityEngine;

public class BombAndBrick : MonoBehaviour
{ 
    private int _x = -1;

    private int _y = -1;

    private static readonly int Destroy1 = Animator.StringToHash("destroy");

    public void Trigger(int x, int y)
    {
        var anim = GetComponent<Animator>();
        anim.SetTrigger(Destroy1);
        _x = x;
        _y = y;
    }
    private void OnDestroy()
    {
        
        MainLogic mainLogic = null;
        var spawnerGameObject = GameObject.Find("spawner");
        if (null != spawnerGameObject)
        {
            mainLogic = spawnerGameObject.GetComponent<MainLogic>();
        }
        if (null != mainLogic)
        {
            mainLogic.DeleteFromGrid(_x,_y);
        }
    }
}
