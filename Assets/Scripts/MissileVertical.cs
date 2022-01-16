
using UnityEngine;

public class MissileVertical : GameElement
{
    public override void OnMouseDown()
    {
        var mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetMissiledBrickUpside(gameObject.transform);
    }
}