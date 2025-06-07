using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // <-- Add this line

public class ImageRotator : MonoBehaviour
{
    public RawImage mainImageDisplay;
    public RawImage secondaryImageDisplay;
    public FileImporter fileImporter; // Reference to use SetAspectRatio()
    

    public void RotateImageObject()
    {
        if (mainImageDisplay != null)
        {
            mainImageDisplay.rectTransform.Rotate(0f, 0f, -90f);
            fileImporter.SetAspectRatio(mainImageDisplay, (Texture2D)mainImageDisplay.texture);
        }

        if (secondaryImageDisplay != null)
        {
            secondaryImageDisplay.rectTransform.Rotate(0f, 0f, -90f);
            fileImporter.SetAspectRatio(secondaryImageDisplay, (Texture2D)secondaryImageDisplay.texture);
        }
    }
}



