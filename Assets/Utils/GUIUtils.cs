using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class GUIUtils
{
    public static GameObject GetGUIObjectAtPosition(Vector3 position)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            return results[0].gameObject;
        }
        return null;
    }
}
