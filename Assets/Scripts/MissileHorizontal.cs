using UnityEngine;

public class MissileHorizontal : MonoBehaviour
{
    public void OnMouseDown()
    { 
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.GetMissiledBrick(gameObject.transform);
    }
}
