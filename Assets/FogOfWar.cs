using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FogOfWar : MonoBehaviour
{
    public RawImage mainImageDisplay;  // Main screen's RawImage
    public RawImage secondaryImageDisplay;  // Secondary screen's RawImage
    public Texture2D overlayTexture;  // Texture for the fog/overlay
    public Color brushColor = new Color(0, 0, 0, 0);  // Transparent color for erasing
    public float brushSize = 20f;  // Size of the brush

    private Texture2D fogTexture;
    private bool isDrawing = false;

    void Start()
    {
        InitializeFogTexture();

        // Assign fog texture to both canvases
        ApplyFogTexture(mainImageDisplay, 0.5f);  // Semi-transparent
        ApplyFogTexture(secondaryImageDisplay, 1f);  // Fully opaque
    }

    void Update()
    {
        if (Input.GetMouseButton(0))  // Left mouse button for drawing
        {
            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainImageDisplay.rectTransform,
                Input.mousePosition,
                null,
                out localMousePosition
            );

            if (IsWithinBounds(localMousePosition, mainImageDisplay.rectTransform))
            {
                Vector2 texCoord = ConvertToTextureCoordinate(localMousePosition, mainImageDisplay.rectTransform, fogTexture);
                EraseAt(texCoord);
            }
        }
    }

    private void InitializeFogTexture()
{
    if (!(overlayTexture is Texture2D))
    {
        Debug.LogError("The overlayTexture must be a Texture2D.");
        return;
    }

    // Cast the overlayTexture to Texture2D
    Texture2D sourceTexture = (Texture2D)overlayTexture;

    // Create a new Texture2D with the same dimensions and format as the source
    fogTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

    // Copy pixels from the source texture to the new fog texture
    Color[] pixels = sourceTexture.GetPixels();
    fogTexture.SetPixels(pixels);
    fogTexture.Apply();
}




    private void ApplyFogTexture(RawImage imageDisplay, float alpha)
    {
        // Apply the texture to the RawImage's material
        if (imageDisplay != null)
        {
            imageDisplay.texture = fogTexture;

            // Set transparency using the color of the RawImage
            imageDisplay.color = new Color(1, 1, 1, alpha);
        }
    }

    private bool IsWithinBounds(Vector2 position, RectTransform rectTransform)
    {
        return position.x >= rectTransform.rect.xMin && position.x <= rectTransform.rect.xMax &&
               position.y >= rectTransform.rect.yMin && position.y <= rectTransform.rect.yMax;
    }

    private Vector2 ConvertToTextureCoordinate(Vector2 localPosition, RectTransform rectTransform, Texture2D texture)
    {
        float x = Mathf.InverseLerp(rectTransform.rect.xMin, rectTransform.rect.xMax, localPosition.x);
        float y = Mathf.InverseLerp(rectTransform.rect.yMin, rectTransform.rect.yMax, localPosition.y);
        return new Vector2(x * texture.width, y * texture.height);
    }

    private void EraseAt(Vector2 texCoord)
    {
        int x = Mathf.Clamp((int)texCoord.x, 0, fogTexture.width - 1);
        int y = Mathf.Clamp((int)texCoord.y, 0, fogTexture.height - 1);

        for (int i = -Mathf.FloorToInt(brushSize); i <= Mathf.FloorToInt(brushSize); i++)
        {
            for (int j = -Mathf.FloorToInt(brushSize); j <= Mathf.FloorToInt(brushSize); j++)
            {
                int pixelX = Mathf.Clamp(x + i, 0, fogTexture.width - 1);
                int pixelY = Mathf.Clamp(y + j, 0, fogTexture.height - 1);

                // Calculate distance to the center of the brush
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(pixelX, pixelY));
                if (distance <= brushSize)
                {
                    // Erase by setting the pixel to transparent
                    fogTexture.SetPixel(pixelX, pixelY, brushColor);
                }
            }
        }

        fogTexture.Apply();
    }
}
