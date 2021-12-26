using UnityEngine;

public class Bomb : MonoBehaviour
{
    public void OnMouseDown()
    {
        MainLogic mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetBombedBrick(gameObject.transform);
    }
}
