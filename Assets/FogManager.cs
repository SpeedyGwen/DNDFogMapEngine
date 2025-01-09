using UnityEngine;
using UnityEngine.UI;

public class FogManager : MonoBehaviour
{
    public RawImage mainImageDisplay;  // Main image display (for the first canvas)
    public RawImage secondaryImageDisplay;  // Secondary image display (for the second canvas)

    public GameObject brushOverlay;  // UI Object (circle) showing brush size
    public float minBrushSize = 5f;  // Minimum brush size
    public float maxBrushSize = 50f; // Maximum brush size

    private Texture2D overlayTexture1;
    private Texture2D overlayTexture2;

    private RectTransform overlayRect1;
    private RectTransform overlayRect2;

    private Camera uiCamera1;
    private Camera uiCamera2;

    private bool isDrawing = false;
    private bool isRightClickDrawing = false;

    private float currentBrushSize = 20f;  // Default brush size

    public int fogTextureWidth = 1920;  // Lower resolution for the fog texture
    public int fogTextureHeight = 1080;

    void Start()
    {
        // Initialize fog overlays for both canvases
        InitializeOverlay(mainImageDisplay, ref overlayTexture1, out overlayRect1);
        InitializeOverlay(secondaryImageDisplay, ref overlayTexture2, out overlayRect2);

        // Cameras to handle the canvas rendering
        uiCamera1 = mainImageDisplay.canvas.worldCamera;
        uiCamera2 = secondaryImageDisplay.canvas.worldCamera;

        // Set initial brush overlay size
        SetBrushOverlaySize(currentBrushSize);
    }

    void Update()
    {
        // Handle mouse scroll to adjust brush size
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            AdjustBrushSize(scrollInput);
        }

        // Update the brush overlay position
        UpdateBrushOverlayPosition();

        // Handle left-click (removing fog) and right-click (adding fog)
        if (Input.GetMouseButton(0)) // Left mouse button (Remove fog)
        {
            if (!isDrawing)
            {
                isDrawing = true;
            }
            PaintFog(overlayRect1, overlayTexture1, uiCamera1, false);  // False for removing fog (transparent)
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            CopyTextureToSecondCanvas();
        }

        if (Input.GetMouseButton(1)) // Right mouse button (Add fog)
        {
            if (!isRightClickDrawing)
            {
                isRightClickDrawing = true;
            }
            PaintFog(overlayRect1, overlayTexture1, uiCamera1, true);  // True for adding fog (opaque)
        }
        else if (Input.GetMouseButtonUp(1) && isRightClickDrawing)
        {
            isRightClickDrawing = false;
            CopyTextureToSecondCanvas();
        }
    }

    // Function to adjust the brush size based on the scroll input
    void AdjustBrushSize(float scrollInput)
    {
        currentBrushSize = Mathf.Clamp(currentBrushSize + scrollInput * 50f, minBrushSize, maxBrushSize);
        SetBrushOverlaySize(currentBrushSize);
    }

    // Function to update the position of the brush overlay (circle) at the mouse position
    void UpdateBrushOverlayPosition()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect1, Input.mousePosition, uiCamera1, out localPoint);
        brushOverlay.transform.position = overlayRect1.TransformPoint(localPoint);
    }

    // Function to update the size of the brush overlay (circle)
    void SetBrushOverlaySize(float brushSize)
    {
        RectTransform brushRect = brushOverlay.GetComponent<RectTransform>();
        brushRect.sizeDelta = new Vector2(brushSize * 2f, brushSize * 2f);  // Set diameter size
    }

    // Function to paint fog (either add or remove based on the boolean flag)
    void PaintFog(RectTransform overlayRect, Texture2D overlayTexture, Camera uiCamera, bool addFog)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, Input.mousePosition, uiCamera, out localPoint);

        if (overlayRect.rect.Contains(localPoint))
        {
            // Normalize mouse position to be in the range of 0-1 on the canvas
            float textureX = Mathf.Clamp01((localPoint.x + overlayRect.rect.width / 2) / overlayRect.rect.width);
            float textureY = Mathf.Clamp01((localPoint.y + overlayRect.rect.height / 2) / overlayRect.rect.height);

            // Map the mouse position to the lower-resolution fog texture
            float fogX = textureX * overlayTexture.width;
            float fogY = textureY * overlayTexture.height;

            int radius = Mathf.RoundToInt(currentBrushSize); // Use current brush size as radius
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int px = Mathf.RoundToInt(fogX + dx);
                    int py = Mathf.RoundToInt(fogY + dy);

                    if (px >= 0 && px < overlayTexture.width && py >= 0 && py < overlayTexture.height)
                    {
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distance <= radius)
                        {
                            Color currentColor = overlayTexture.GetPixel(px, py);
                            if (addFog) 
                            {
                                currentColor.a = 1f;  // Full fog (opaque)
                            }
                            else 
                            {
                                currentColor.a = 0f;  // No fog (transparent)
                            }
                            overlayTexture.SetPixel(px, py, currentColor);
                        }
                    }
                }
            }
            overlayTexture.Apply(); // Apply the changes to the texture
        }
    }

    // Copy the fog texture from the first canvas to the second canvas
    void CopyTextureToSecondCanvas()
    {
        // Simply copy the texture from the first to the second canvas
        overlayTexture2.SetPixels(overlayTexture1.GetPixels());
        overlayTexture2.Apply();
    }

    // Initialize the fog texture overlay
    void InitializeOverlay(RawImage overlay, ref Texture2D overlayTexture, out RectTransform overlayRect)
    {
        overlayRect = overlay.GetComponent<RectTransform>();

        // Initialize the fog texture with fixed resolution (no scaling)
        overlayTexture = new Texture2D(fogTextureWidth, fogTextureHeight);
        ResetFogTexture(overlayTexture);
        overlay.texture = overlayTexture;
    }

    // Reset the fog texture to full opacity
    void ResetFogTexture(Texture2D texture)
    {
        Color[] pixels = new Color[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white; // Set all pixels to opaque (no fog)
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    // Reset fog when a new image is imported
    public void ResetFogOnImageImport(Texture2D newImageTexture)
    {
        // Reset the fog texture to its initial state when a new image is imported
        ResetFogTexture(overlayTexture1);
        ResetFogTexture(overlayTexture2);

        // Ensure drawing starts as false after reset
        isDrawing = false;
        isRightClickDrawing = false;
    }
}
