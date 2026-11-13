using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    GameObject tile, bottomTile, startButton;

    TMP_Text scoreText;
    List<GameObject> stack;

    bool hasGameStarted, hasGameFinished;
    [Header("Camera Settings")]
    [SerializeField] private float cameraMoveDistance = 0.5f;
    [SerializeField] private float cameraMoveSpeed = 2.0f;
    [Header("Game Over Camera Settings")]
    [SerializeField] private float zoomOutDuration = 1.5f;
    [SerializeField] private float zoomPadding = 1.2f;

    List<Color32> spectrum = new List<Color32>(){
        new Color32(0, 255, 33, 255)    ,
        new Color32(167, 255, 0, 255)   ,
        new Color32(230, 255, 0, 255)   ,
        new Color32(255, 237, 0, 255)   ,
        new Color32(255, 206, 0, 255)   ,
        new Color32(255, 185, 0, 255)   ,
        new Color32(255, 142, 0, 255)   ,
        new Color32(255, 111, 0, 255)   ,
        new Color32(255, 58, 0, 255)    ,
        new Color32(255, 0, 0, 255)     ,
        new Color32(255, 0, 121, 255)   ,
        new Color32(255, 0, 164, 255)   ,
        new Color32(241, 0, 255, 255)   ,
        new Color32(209, 0, 255, 255)   ,
        new Color32(178, 0, 255, 255)   };
    int modifier;
    int colorIndex;

    public static Spawner instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
        stack = new List<GameObject>();
        hasGameFinished = false;
        hasGameStarted = false;

        modifier = Random.value < 0.5f ? 1 : -1;

        // Pick a random starting color
        colorIndex = Random.Range(0, spectrum.Count);

        stack.Add(bottomTile);

        // Apply the random color to the bottom tile
        stack[0].GetComponent<Renderer>().material.color = spectrum[colorIndex];

        CreateTile();

    }

    // Update is called once per frame


    void Update()
    {
        // Test whether Unity detects the tap
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("SCREEN TAPPED!");
        }

        // Check if the game is allowed to run
        if (hasGameFinished || !hasGameStarted)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {

            if (stack.Count > 1)
            {
                GameObject currentTile = stack[stack.Count - 1];

                currentTile.GetComponent<Tile>().ScaleTile();

                // Only play the echo on a successful placement
                if (!hasGameFinished)
                {
                    PlacementEcho echo =
                        currentTile.GetComponent<PlacementEcho>();

                    if (echo != null)
                        echo.PlayEcho();
                }
            }


            if (hasGameFinished)
                return;

            StartCoroutine(MoveCamera());
            scoreText.text = (stack.Count - 1).ToString();
            CreateTile();
        }
    }


    bool ScreenTapped()
    {
        // Detect actual touchscreen input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                return true;
        }

        // Detect mouse clicks in Unity Editor / Device Simulator
        if (Input.GetMouseButtonDown(0))
            return true;

        return false;
    }



    IEnumerator MoveCamera()
    {
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

        Vector3 startPosition = camera.transform.position;
        Vector3 targetPosition = startPosition +
            Vector3.up * cameraMoveDistance;

        float elapsedTime = 0f;
        float duration = cameraMoveDistance / Mathf.Max(cameraMoveSpeed, 0.01f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            camera.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        camera.transform.position = targetPosition;
    }


    void CreateTile()
    {
        GameObject previousTile = stack[stack.Count - 1];
        GameObject activeTile;

        activeTile = Instantiate(tile);
        stack.Add(activeTile);

        if (stack.Count > 2)
            activeTile.transform.localScale = previousTile.transform.localScale;

        activeTile.transform.position = new Vector3(previousTile.transform.position.x,
            previousTile.transform.position.y + previousTile.transform.localScale.y, previousTile.transform.position.z);

        colorIndex += modifier;
        if (colorIndex == spectrum.Count || colorIndex == -1)
        {
            modifier *= -1;
            colorIndex += 2 * modifier;
        }

        activeTile.GetComponent<Renderer>().material.color = spectrum[colorIndex];
        activeTile.GetComponent<Tile>().moveX = stack.Count % 2 == 0;
    }

    public void GameOver()
    {
        startButton.SetActive(true);
        hasGameFinished = true;
        StartCoroutine(EndCamera());
    }


    IEnumerator EndCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
            yield break;

        // Find the bounds of the stacked tiles
        Bounds towerBounds = stack[0].GetComponent<Renderer>().bounds;

        foreach (GameObject tile in stack)
        {
            if (tile != null)
            {
                Renderer tileRenderer = tile.GetComponent<Renderer>();

                if (tileRenderer != null)
                    towerBounds.Encapsulate(tileRenderer.bounds);
            }
        }

        // Find the actual center of the tower
        Vector3 towerCenter = towerBounds.center;

        // Keep the camera's original viewing angle
        Vector3 targetPosition = towerCenter
            - cam.transform.forward * 20f;

        // Calculate zoom based on tower height and width
        float verticalSize = towerBounds.size.y / 2f;

        float horizontalSize =
            Mathf.Max(towerBounds.size.x, towerBounds.size.z) /
            (2f * cam.aspect);

        float targetSize = Mathf.Max(
            verticalSize,
            horizontalSize
        ) * zoomPadding;

        // Save starting camera values
        Vector3 startPosition = cam.transform.position;
        float startSize = cam.orthographicSize;

        float elapsed = 0f;

        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / zoomOutDuration);

            // Smooth transition
            t = t * t * (3f - 2f * t);

            cam.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            cam.orthographicSize = Mathf.Lerp(
                startSize,
                targetSize,
                t
            );

            yield return null;
        }

        // Ensure exact final values
        cam.transform.position = targetPosition;
        cam.orthographicSize = targetSize;
    }


    public void StartButton()
    {
        if (hasGameFinished)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
             UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
);
        }
        else
        {
            startButton.SetActive(false);
            hasGameStarted = true;
        }
    }
}