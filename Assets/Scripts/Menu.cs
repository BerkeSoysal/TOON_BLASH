using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour,IPointerDownHandler
{
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        SceneManager.LoadScene("Menu");
    }
}
