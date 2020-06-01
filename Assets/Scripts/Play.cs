using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        SceneManager.LoadScene("Game");
    }
}
