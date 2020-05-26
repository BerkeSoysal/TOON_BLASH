using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAndBrick : MonoBehaviour
{ 
    private int x = -1;

    private int y = -1;

    private Dictionary<int, int> _dictionary;

    private static readonly int Destroy1 = Animator.StringToHash("destroy");
    
    public void trigger(int x, int y, Dictionary<int,int> dictionary)
    {
        var anim = GetComponent<Animator>();
        anim.SetTrigger(Destroy1);
        this.x = x;
        this.y = y;
        _dictionary = dictionary;
    }
    private void OnDestroy()
    {
        BlockSpawner blockSpawner = null;
        var spawnerGameObject = GameObject.Find("spawner");
        if (null != spawnerGameObject)
        {
            blockSpawner = spawnerGameObject.GetComponent<BlockSpawner>();
        }
        if (null != blockSpawner)
        {
            blockSpawner.DeleteFromGrid(x,y, _dictionary);
        }
    }
}
