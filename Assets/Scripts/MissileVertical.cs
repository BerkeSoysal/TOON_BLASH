
using UnityEngine;

public class MissileVertical : MonoBehaviour
{
    public void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getMissiledBrickUpside(gameObject.transform);
    }
}