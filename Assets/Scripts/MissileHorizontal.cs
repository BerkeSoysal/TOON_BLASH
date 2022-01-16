using UnityEngine;

public class MissileHorizontal : GameElement
{
    public override void OnMouseDown()
    { 
        var mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetMissiledBrick(gameObject.transform);
    }
}
