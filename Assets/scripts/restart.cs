using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class restart : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        SceneManager.LoadScene("Game");
    }
}
