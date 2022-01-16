using UnityEngine;

public class Brick : MonoBehaviour
{    
    private void OnMouseDown()
    {
        var mainLogic = GameObject.Find("spawner").GetComponent<MainLogic>();
        mainLogic.GetClickedBrick(gameObject.transform);
    }
}
