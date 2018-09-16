using UnityEngine;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour
{
    public Image rangeIndicator;

    public void ShowMovementHelp(float xPosition, float radius)
    {
        rangeIndicator.rectTransform.sizeDelta = new Vector2(2 * radius, rangeIndicator.rectTransform.sizeDelta.y);

        rangeIndicator.rectTransform.position = new Vector3(
            xPosition,
            rangeIndicator.rectTransform.position.y,
            rangeIndicator.rectTransform.position.z);

        rangeIndicator.gameObject.SetActive(true);
    }

    public void HideMovementHelp()
    {
        rangeIndicator.gameObject.SetActive(false);
    }
}
