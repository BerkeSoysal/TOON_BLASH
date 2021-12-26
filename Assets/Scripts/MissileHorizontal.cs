using UnityEngine;

public class MissileHorizontal : MonoBehaviour
{
    public void OnMouseDown()
    { 
        MainLogic mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetMissiledBrick(gameObject.transform);
    }
}
