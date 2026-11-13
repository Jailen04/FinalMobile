
using System.Collections;
using UnityEngine;

public class PlacementEcho : MonoBehaviour
{
    [Header("Echo Settings")]
    [SerializeField] private Material echoMaterial;
    [SerializeField] private float duration = 0.45f;
    [SerializeField] private float expansion = 1.35f;
    [SerializeField] private float startingAlpha = 0.8f;
    [SerializeField] private float thickness = 0.035f;
    [SerializeField] private float verticalOffset = 0.04f;

    public void PlayEcho()
    {
        if (echoMaterial == null)
        {
            Debug.LogWarning("Assign an Echo Material!");
            return;
        }

        StartCoroutine(AnimateEcho());
    }

    IEnumerator AnimateEcho()
    {
        Renderer sourceRenderer = GetComponent<Renderer>();
        if (sourceRenderer == null)
            yield break;

        // Match the color of the placed tile
        Color tileColor = sourceRenderer.material.color;

        // Create a thin silhouette beneath the tile
        GameObject echo = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        echo.name = "Placement Echo";

        Destroy(echo.GetComponent<Collider>());

        echo.transform.position = transform.position
            - Vector3.up * (transform.localScale.y / 2f
            + verticalOffset);

        echo.transform.rotation = transform.rotation;

        Vector3 initialScale = new Vector3(
            transform.localScale.x,
            thickness,
            transform.localScale.z
        );

        echo.transform.localScale = initialScale;

        Renderer echoRenderer = echo.GetComponent<Renderer>();
        Material materialInstance = new Material(echoMaterial);
        echoRenderer.material = materialInstance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed /
                Mathf.Max(duration, 0.001f));

            float scale = Mathf.Lerp(1f, expansion, t);

            echo.transform.localScale = new Vector3(
                initialScale.x * scale,
                thickness,
                initialScale.z * scale
            );

            Color currentColor = tileColor;
            currentColor.a = startingAlpha * (1f - t);

            materialInstance.color = currentColor;

            yield return null;
        }

        Destroy(echo);
        Destroy(materialInstance);
    }
}
