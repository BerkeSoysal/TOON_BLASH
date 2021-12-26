
using UnityEngine;

public class MissileVertical : MonoBehaviour
{
    public void OnMouseDown()
    {
        MainLogic mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetMissiledBrickUpside(gameObject.transform);
    }
}