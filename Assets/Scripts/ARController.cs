using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    public ARRaycastManager raycastManager;  // AR Raycast Manager
    public Camera arCamera;                 // Reference to the AR Camera
    public RawImage displayImage;           // UI RawImage to display the captured image
    public GameObject cubePrefab;           // Cube prefab to instantiate

    private RenderTexture renderTexture;

    void Start()
    {
        // Create a RenderTexture for capturing
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }

    void Update()
    {
        // Place cube on screen tap
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            PlaceCube(touchPosition);
        }
    }

    void PlaceCube(Vector2 touchPosition)
    {
        // Perform a raycast
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Offset the cube's position by 1f along the Y-axis
            Vector3 spawnPosition = hitPose.position + Vector3.up * 0.2f;

            // Instantiate a cube at the offset position
            GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

            // Assign a random color to the cube
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material.color = new Color(Random.value, Random.value, Random.value);

            // Schedule image capture
            StartCoroutine(CaptureImageAfterFrames(10));
        }
    }


    IEnumerator CaptureImageAfterFrames(int frameDelay)
    {
        // Wait for the specified number of frames
        for (int i = 0; i < frameDelay; i++) yield return null;

        // Capture the ARCamera's output
        CaptureARCameraOutput();
    }

    void CaptureARCameraOutput()
    {
        // Set the camera's target texture to the RenderTexture
        arCamera.targetTexture = renderTexture;

        // Render the camera's view into the RenderTexture
        arCamera.Render();

        // Create a Texture2D to store the RenderTexture's data
        Texture2D capturedImage = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // Read pixels from the RenderTexture into the Texture2D
        RenderTexture.active = renderTexture;
        capturedImage.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        capturedImage.Apply();

        // Reset camera's target texture
        arCamera.targetTexture = null;
        RenderTexture.active = null;

        // Display the captured image on the RawImage UI
        displayImage.texture = capturedImage;
    }

    private void OnDestroy()
    {
        // Clean up the RenderTexture
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}
