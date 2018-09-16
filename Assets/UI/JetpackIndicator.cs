using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JetpackIndicator : MonoBehaviour
{

    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float fadeOutAfter = 1f;

    Slider slider;
    Jetpack jetpack;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        jetpack = player.GetComponentInChildren<Jetpack>();
        slider.minValue = 0f;
        slider.maxValue = jetpack.maxCharge;
    }

    Coroutine fadeOutCoroutine;
    Coroutine fadeInCoroutine;

    private float alpha = 1f;

    IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            alpha = Mathf.Lerp(alpha, 1f, (elapsedTime / fadeInDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeInCoroutine = null;
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(fadeOutAfter);
        float elapsedTime = 0;
        while (elapsedTime < fadeOutDuration)
        {
            alpha = Mathf.Lerp(alpha, 0f, (elapsedTime / fadeOutDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeOutCoroutine = null;
    }

    private void Update()
    {

        float current = jetpack.CurrentCharge;

        ColorBlock cb = slider.colors;
        Color lerpedColor = Color.Lerp(Color.red, Color.green, jetpack.CurrentCharge / jetpack.maxCharge);
        lerpedColor.a = alpha;
        cb.normalColor = lerpedColor;
        slider.colors = cb;

        slider.value = current;

        if (jetpack.IsFull())
        {
            if (fadeOutCoroutine == null)
            {
                fadeOutCoroutine = StartCoroutine(FadeOut());
            }
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }
        }
        else
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            if (fadeInCoroutine == null)
            {
                fadeInCoroutine = StartCoroutine(FadeIn());
            }
        }
    }
}
