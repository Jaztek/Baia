using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public bool isTimeSlowedDown = false;
    public Material material;

    public void Start()
    {
        material.SetFloat("_EffectAmount", 0f);
    }

    public void SwapTimeScale()
    {
        isTimeSlowedDown = !isTimeSlowedDown;
        if (isTimeSlowedDown)
        {
            Time.timeScale = 0.1f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            StartCoroutine(lerp(1f));
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02F;
            StartCoroutine(lerp(0f));
        }
    }

    IEnumerator lerp(float target)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            material.SetFloat("_EffectAmount", Mathf.Lerp(material.GetFloat("_EffectAmount"), target, elapsedTime / 0.1f));
            Debug.Log(material.GetFloat("_EffectAmount"));
            yield return null;
        }
    }
}
