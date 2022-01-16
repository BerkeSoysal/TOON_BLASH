using UnityEngine;

public class Bomb : GameElement
{
    public override void OnMouseDown()
    {
        var mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetBombedBrick(gameObject.transform);
    }
}
