using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI.Extensions;

public class Translocator : Equipable
{
    public LayerMask obstacleMask;
    public int maxNumberOfPoints = 10;
    public float throwForce = 15f;

    public GameObject ballPrefab;

    private bool showGUIInfo;
    private int phase = 0;

    // MAGIC NUMBER -- ANALIZAR DE DONDE VIENE
    private float forceFactor;

    private UILineRenderer lineRenderer;
    private UICircle circle;

    private Canvas canvas;
    private Player player;

    private Vector2 initialPosition;
    private GameObject ball;

    public void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        Assert.IsNotNull(canvas);

        player = FindObjectOfType<Player>();
        Assert.IsNotNull(player);

        // ?????????????
        forceFactor = Mathf.Sqrt((Screen.height / 30.012f) + 1.03f);
    }

    public override void OnActionStart(Vector2 position)
    {
        if (phase == 0)
        {
            Debug.Log("ACTION START");
            showGUIInfo = true;
            GameObject objLineRenderer = new GameObject("UILineRenderer");

            if (lineRenderer)
            {
                Destroy(lineRenderer.gameObject);
            }
            lineRenderer = objLineRenderer.AddComponent<UILineRenderer>();
            lineRenderer.GetComponent<RectTransform>().pivot = Vector2.zero;
            objLineRenderer.transform.SetParent(canvas.transform);

            GameObject objCircle = new GameObject("UICircle");

            if (circle)
            {
                Destroy(circle.gameObject);
            }
            circle = objCircle.AddComponent<UICircle>();
            circle.GetComponent<RectTransform>().sizeDelta = 30 * Vector2.one;
            objCircle.transform.SetParent(canvas.transform);

            initialPosition = position;
            objCircle.transform.position = initialPosition;
        }
        else if (phase == 1)
        {
            if (ball)
            {
                player.transform.position = ball.transform.position;
                Destroy(ball.gameObject);
            }
        }
    }

    public override void OnActionEnd()
    {
        if (phase == 0)
        {
            showGUIInfo = false;
            Debug.Log("ACTION END");
            Destroy(lineRenderer.gameObject);
            Destroy(circle.gameObject);

            Vector2 diff = (currentPosition - initialPosition).normalized;

            ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
            ball.GetComponent<Rigidbody2D>().AddTorque(UnityEngine.Random.Range(-30, 30));
            ball.GetComponent<Rigidbody2D>().AddForce(throwForce * diff, ForceMode2D.Impulse);

            phase = 1;
        }
        else if (phase == 1)
        {
            phase = 0;
        }
    }

    private void LateUpdate()
    {
        if (showGUIInfo)
        {
            Vector2 zeroPosition = Camera.main.WorldToScreenPoint(transform.position);

            Vector2 diff = throwForce * forceFactor * (currentPosition - initialPosition).normalized;
            float v0x = diff.x;
            float v0y = diff.y;

            float fTime = 0f;
            float increment = 0.1f;

            Vector2[] points = new Vector2[maxNumberOfPoints];

            for (int i = 0; i < maxNumberOfPoints; i++)
            {
                Vector2 r = new Vector2(v0x * fTime + zeroPosition.x,
                    0.5f * Physics2D.gravity.y * fTime * fTime + v0y * fTime + zeroPosition.y);

                points[i] = r;
                fTime += increment;

                if (Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(r), obstacleMask))
                {
                    Array.Resize(ref points, i);
                    break;
                }
            }
            lineRenderer.Points = points;
            lineRenderer.SetAllDirty();
        }
    }
}
