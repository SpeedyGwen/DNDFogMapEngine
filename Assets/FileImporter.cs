using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SFB; // Import for UnityStandaloneFileBrowser

public class FileImporter : MonoBehaviour
{
    public RawImage mainImageDisplay;  // RawImage in the main canvas
    public RawImage secondaryImageDisplay;  // RawImage in the second canvas
    public FogManager fogManager;  // Reference to the FogManager

    public void OnImportButtonClick()
    {
        // Open the file dialog
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select an Image", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            StartCoroutine(LoadImage(paths[0]));
        }
        else
        {
            Debug.LogWarning("No file selected or operation canceled.");
        }
    }

    private IEnumerator LoadImage(string filePath)
{
    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + filePath))
    {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading image: " + www.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            if (mainImageDisplay != null)
            {
                // Apply texture to the first RawImage
                mainImageDisplay.texture = texture;
                SetAspectRatio(mainImageDisplay, texture);

                // Apply texture to the second RawImage
                if (secondaryImageDisplay != null)
                {
                    secondaryImageDisplay.texture = texture;
                    SetAspectRatio(secondaryImageDisplay, texture);
                }
                else
                {
                    Debug.LogWarning("Secondary RawImage is not assigned.");
                }

                Debug.Log("Image loaded and applied to both canvases.");

                // Reset the fog and resize to match the new image
                if (fogManager != null)
                {
                    fogManager.ResetFogOnImageImport(texture); // Pass the texture to resize the fog
                }
            }
            else
            {
                Debug.LogError("Main RawImage is not assigned.");
            }
        }
    }
}



    private void SetAspectRatio(RawImage imageDisplay, Texture2D texture)
    {
        // Maintain aspect ratio
        AspectRatioFitter aspectFitter = imageDisplay.GetComponent<AspectRatioFitter>();
        if (aspectFitter == null)
        {
            aspectFitter = imageDisplay.gameObject.AddComponent<AspectRatioFitter>();
        }
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

        // Update aspect ratio based on texture dimensions
        float aspectRatio = (float)texture.width / texture.height;
        aspectFitter.aspectRatio = aspectRatio;

        // Center the image
        RectTransform rectTransform = imageDisplay.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}