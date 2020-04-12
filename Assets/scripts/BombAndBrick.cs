using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombAndBrick : MonoBehaviour
{ 
    private int x = -1;

    private int y = -1;

    private Dictionary<int, int> _dictionary;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void trigger(int x, int y, Dictionary<int,int> dictionary, bool fromBomb)
    {
        var anim = GetComponent<Animator>();
        anim.SetTrigger("destroy");
        this.x = x;
        this.y = y;
        this._dictionary = dictionary;
    }
    // Update is called once per frame
    void Update()
    {
        
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
            blockSpawner.deleteFromGrid(x,y, _dictionary);
        }
    }
}
