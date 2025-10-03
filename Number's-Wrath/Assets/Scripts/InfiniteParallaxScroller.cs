using System.Collections.Generic;
using UnityEngine;

public class InfiniteParallaxScroller : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab de segmento: debe tener SpriteRenderer con sprite seamless.")]
    public GameObject segmentPrefab;
    [Tooltip("Transform de la cámara (normalmente Camera.main.transform).")]
    public Transform followCamera;

    [Header("Layout")]
    [Tooltip("Cuántos segmentos iniciales crear (mínimo 3 recomendado).")]
    public int initialSegments = 5;
    [Tooltip("Espacio extra (padding) entre segmentos, en unidades world. Usar 0 para pegado.")]
    public float segmentPadding = 0f;

    [Header("Behaviour")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;
    public bool horizontal = true;
    public bool alsoParallaxY = false;

    List<Transform> segments = new List<Transform>();
    float segmentSize = 1f;
    Vector3 previousCamPos;

    void Start()
    {
        if (segmentPrefab == null)
        {
            Debug.LogError("[InfiniteParallaxScroller] segmentPrefab no asignado.");
            enabled = false;
            return;
        }

        if (followCamera == null && Camera.main != null) followCamera = Camera.main.transform;
        if (followCamera == null)
        {
            Debug.LogError("[InfiniteParallaxScroller] followCamera no asignado y Camera.main es null.");
            enabled = false;
            return;
        }

        var spr = segmentPrefab.GetComponentInChildren<SpriteRenderer>();
        if (spr == null)
        {
            Debug.LogError("[InfiniteParallaxScroller] segmentPrefab no tiene SpriteRenderer hijo.");
            enabled = false;
            return;
        }

        GameObject probe = Instantiate(segmentPrefab, Vector3.zero, Quaternion.identity);
        SpriteRenderer probeSR = probe.GetComponentInChildren<SpriteRenderer>();
        if (probeSR == null)
        {
            Debug.LogError("[InfiniteParallaxScroller] probe no encontró SpriteRenderer.");
            Destroy(probe);
            enabled = false;
            return;
        }

        segmentSize = horizontal ? probeSR.bounds.size.x : probeSR.bounds.size.y;
        Destroy(probe);

        if (segmentSize <= 0.0001f)
        {
            Debug.LogError("[InfiniteParallaxScroller] segmentSize calculado = 0. Revisa escala/sprite.");
            enabled = false;
            return;
        }

        float startPos = horizontal ? followCamera.position.x : followCamera.position.y;
        
        float half = initialSegments / 2f;
        for (int i = 0; i < initialSegments; i++)
        {
            float offsetIndex = (i - half + 0.5f); 
            Vector3 worldPos = transform.position;
            if (horizontal)
                worldPos.x = (startPos + offsetIndex * (segmentSize + segmentPadding));
            else
                worldPos.y = (startPos + offsetIndex * (segmentSize + segmentPadding));

            GameObject g = Instantiate(segmentPrefab, worldPos, Quaternion.identity, transform);

            g.transform.localScale = Vector3.one;
            segments.Add(g.transform);
        }

        segments.Sort((a, b) => (horizontal ? a.position.x.CompareTo(b.position.x) : a.position.y.CompareTo(b.position.y)));

        previousCamPos = followCamera.position;
    }

    void LateUpdate()
    {
        if (segments.Count == 0) return;
        Vector3 camPos = followCamera.position;
        Vector3 camDelta = camPos - previousCamPos;

        Vector3 parallaxMove = new Vector3(
            // X
            (alsoParallaxY || horizontal) ? camDelta.x * parallaxFactor : 0f,
            // Y
            (alsoParallaxY || !horizontal) ? camDelta.y * (alsoParallaxY ? parallaxFactor : 0f) : 0f,
            0f
        );
        transform.position += parallaxMove;

        if (horizontal)
        {
            Camera cam = Camera.main;
            float camHalfWidth = cam.orthographicSize * cam.aspect;
            float camRight = camPos.x + camHalfWidth;
            float camLeft = camPos.x - camHalfWidth;

            Transform leftmost = segments[0];
            Transform rightmost = segments[segments.Count - 1];

            float leftmostRightEdge = leftmost.position.x + (segmentSize / 2f);
            float rightmostLeftEdge = rightmost.position.x - (segmentSize / 2f);

            float recycleBuffer = segmentSize * 0.5f;

            if (camPos.x > previousCamPos.x && (leftmostRightEdge < camLeft - recycleBuffer))
            {
                float newX = rightmost.position.x + segmentSize + segmentPadding;
                leftmost.position = new Vector3(newX, leftmost.position.y, leftmost.position.z);

                segments.RemoveAt(0);
                segments.Add(leftmost);
            }
            else if (camPos.x < previousCamPos.x && (rightmostLeftEdge > camRight + recycleBuffer))
            {
                float newX = leftmost.position.x - segmentSize - segmentPadding;
                rightmost.position = new Vector3(newX, rightmost.position.y, rightmost.position.z);

                segments.RemoveAt(segments.Count - 1);
                segments.Insert(0, rightmost);
            }
        }
        else
        {
            Camera cam = Camera.main;
            float camHalfHeight = cam.orthographicSize;
            float camTop = camPos.y + camHalfHeight;
            float camBottom = camPos.y - camHalfHeight;

            Transform bottomMost = segments[0];
            Transform topMost = segments[segments.Count - 1];

            float bottomMostTopEdge = bottomMost.position.y + (segmentSize / 2f);
            float topMostBottomEdge = topMost.position.y - (segmentSize / 2f);
            float recycleBuffer = segmentSize * 0.5f;

            if (camPos.y > previousCamPos.y && (bottomMostTopEdge < camBottom - recycleBuffer))
            {
                float newY = topMost.position.y + segmentSize + segmentPadding;
                bottomMost.position = new Vector3(bottomMost.position.x, newY, bottomMost.position.z);

                segments.RemoveAt(0);
                segments.Add(bottomMost);
            }
            else if (camPos.y < previousCamPos.y && (topMostBottomEdge > camTop + recycleBuffer))
            {
                float newY = bottomMost.position.y - segmentSize - segmentPadding;
                topMost.position = new Vector3(topMost.position.x, newY, topMost.position.z);

                segments.RemoveAt(segments.Count - 1);
                segments.Insert(0, topMost);
            }
        }

        previousCamPos = camPos;
    }

    void OnDrawGizmosSelected()
    {
        if (followCamera == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(followCamera.position, 0.1f);
    }
}
