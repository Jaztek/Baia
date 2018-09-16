using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool displayFPS;

    bool isTimeSlowedDown = false;
    public Material material;

    ICollection<string> requestors;

    static GameController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (!displayFPS)
        {
            return;
        }

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 30;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }

    public void Start()
    {
        requestors = new HashSet<string>();
        material.SetFloat("_EffectAmount", 0f);
    }

    public void SwapTimeScale()
    {
        isTimeSlowedDown = !isTimeSlowedDown;
        if (isTimeSlowedDown)
        {
            Time.timeScale = 0.1f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            StartCoroutine(Lerp(1f));
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02F;
            StartCoroutine(Lerp(0f));
        }
    }

    // Envía una petición de parada del slowmo
    public void RequestStopSlowmo(string requestedBy)
    {
        requestors.Remove(requestedBy);
        // Si el tiempo ya estaba a velocidad normal o había algun requester además de este salimos
        if (!isTimeSlowedDown || requestors.Count > 0)
        {
            return;
        }
        SwapTimeScale();
    }

    // Envía una petición de activación del slowmo
    public void RequestStartSlowmo(string requestedBy)
    {
        requestors.Add(requestedBy);
        // Si ya estaba activo no hacemos nada
        if (isTimeSlowedDown)
        {
            return;
        }
        SwapTimeScale();
    }

    public void ApplyDamage(GameObject source, GameObject target, float ammount, Vector2 direction)
    {
        if (source.CompareTag("Player") && target.CompareTag("Enemy"))
        {
            Enemy e = target.GetComponent<Enemy>();
            e.ReceiveDamage(ammount, direction);
        }
        else if (source.CompareTag("Enemy") && target.CompareTag("Player"))
        {
            DamageData data = new DamageData()
            {
                ammount = ammount,
                direction = direction
            };

            target.SendMessage("ReceiveDamage", data);
        }
    }

    public static GameController GetInstance()
    {
        return instance;
    }

    IEnumerator Lerp(float target)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            material.SetFloat("_EffectAmount", Mathf.Lerp(material.GetFloat("_EffectAmount"), target, elapsedTime / 0.1f));
            yield return null;
        }
    }
}
