using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;

public class Bow : Equipable
{
    public float damage = 1f;
    public float quickShotForce = 20f;
    public LayerMask enemiesMask;
    public LayerMask groundMask;

    public GameObject arrowPrefab;

    GameObject player;
    Camera mainCamera;

    UILineRenderer lineRenderer;

    private Vector2 initialPosition;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");

        GameObject objLineRenderer = new GameObject("UILineRenderer");
        lineRenderer = objLineRenderer.AddComponent<UILineRenderer>();
        lineRenderer.GetComponent<RectTransform>().pivot = Vector2.zero;

        Canvas c = FindObjectOfType<Canvas>();
        objLineRenderer.transform.SetParent(c.transform);

        objLineRenderer.SetActive(false);
    }

    private void LateUpdate()
    {
        Vector2[] points = new Vector2[] { initialPosition, currentPosition };
        lineRenderer.Points = points;
        lineRenderer.SetAllDirty();
    }

    public override void OnActionStart(Vector2 position)
    {
        initialPosition = position;
        currentPosition = position;
        lineRenderer.gameObject.SetActive(true);
    }

    public override void OnActionEnd()
    {
        Vector2 diff = currentPosition - initialPosition;
        float angle = Mathf.Atan2(diff.y, diff.x);

        Shot(angle);
        lineRenderer.gameObject.SetActive(false);
    }

    private void Shot(float angle)
    {
        GameObject arrow = Instantiate(arrowPrefab, player.transform.position, Quaternion.identity);

        arrow.GetComponent<Arrow>().SetDamage(damage);
        arrow.GetComponent<Rigidbody2D>().velocity = quickShotForce * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void QuickShot()
    {
        Vector3 playerPosition = player.transform.position;

        Vector3 halfCameraBounds = new Vector3(
            mainCamera.orthographicSize * Screen.width / Screen.height,
            mainCamera.orthographicSize);

        Collider2D[] collidersInView = Physics2D.OverlapAreaAll(
            mainCamera.transform.position - halfCameraBounds,
            mainCamera.transform.position + halfCameraBounds,
            enemiesMask);

        GameObject[] enemies = collidersInView.
            Select(c => c.gameObject).ToArray();

        // Fisher-Yates shuffle algorithm
        for (int i = 0; i < enemies.Length; i++)
        {
            int r = UnityEngine.Random.Range(0, i);
            GameObject tmp = enemies[i];
            enemies[i] = enemies[r];
            enemies[r] = tmp;
        }

        bool isFacingRight = true;
        // TODO fix this characterController.IsFacingRight;

        GameObject firstInvalidCandidate = null;

        bool foundSolution = false;

        foreach (GameObject enemy in enemies)
        {
            if (isFacingRight && enemy.transform.position.x < playerPosition.x
                || !isFacingRight && enemy.transform.position.x > playerPosition.x)
            {
                continue;
            }

            RaycastHit2D raycastHit = Physics2D.Raycast(
                playerPosition,
                enemy.transform.position - playerPosition,
                10,
                groundMask);

            Debug.DrawRay(playerPosition, 10 * (enemy.transform.position - playerPosition), Color.red, 1f);

            if (raycastHit)
            {
                // En este caso es mejor buscar otro candidato; 
                // si no encontramos ninguno tendremos que usar este
                if (firstInvalidCandidate == null)
                {
                    firstInvalidCandidate = enemy;
                }
                continue;
            }

            foundSolution = SpawnArrow(player.transform, enemy.transform, isFacingRight);
            if (foundSolution)
            {
                break;
            }
            else
            {
                if (firstInvalidCandidate == null)
                {
                    firstInvalidCandidate = enemy;
                }
            }
        }

        if (!foundSolution)
        {
            if (firstInvalidCandidate)
            {
                SpawnArrow(player.transform, firstInvalidCandidate.transform, isFacingRight);
            }
            else
            {
                SpawnArrow(player.transform, null, isFacingRight);
            }
        }
    }

    private bool SpawnArrow(Transform player, Transform target, bool isFacingRight)
    {
        float angle = 0f;

        if (target)
        {

            float v = quickShotForce;
            float x = (target.position.x - player.position.x);
            float y = (target.position.y - player.position.y);
            float g = -Physics2D.gravity.y;

            float sqrt = (v * v * v * v) - g * (g * x * x + 2 * y * v * v);
            if (sqrt < 0)
            {
                // TODO ¿Qué hacemos si no encuentra solución?
                Debug.Log("NO SOLUTION");
                return false;
            }

            sqrt = Mathf.Sqrt(sqrt);

            // TODO tenemos que quedarnos con la solución más corta
            bool firstSolution = false;

            angle = Mathf.Atan((v * v + (firstSolution ? sqrt : -sqrt)) / (g * x));
        }

        if (!isFacingRight)
        {
            angle += Mathf.PI;
        }

        GameObject arrow = Instantiate(arrowPrefab, player.position, Quaternion.identity);
        arrow.GetComponent<Rigidbody2D>().velocity = quickShotForce * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return true;
    }
}
