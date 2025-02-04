using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIFirstSelected : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void OnDisable()
    {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
    }
}
