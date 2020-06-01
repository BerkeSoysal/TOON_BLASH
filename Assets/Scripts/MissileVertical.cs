
using UnityEngine;

public class MissileUpside : MonoBehaviour
{
    public void OnMouseDown()
    {
        BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
        blockSpawner.getMissiledBrickUpside(gameObject.transform);
    }
}