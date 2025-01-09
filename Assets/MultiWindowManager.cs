using UnityEngine;

public class MultiWindowManager : MonoBehaviour
{
    public Camera primaryCamera;      // Camera for the primary screen (1st monitor)
    public Camera secondaryCamera;    // Camera for the secondary screen (2nd monitor)
    public Canvas primaryCanvas;      // Canvas for the primary screen
    public Canvas secondaryCanvas;    // Canvas for the secondary screen

    [System.Obsolete]
    void Start()
    {
        // Set the primary camera to render to the first display (default)
        primaryCamera.targetDisplay = 0;
        primaryCanvas.worldCamera = primaryCamera;

        // Ensure the first window is in windowed full-screen mode
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow, 60);

        // Check if we have more than one display
        if (Display.displays.Length > 1)
        {
            // Activate the second display
            Display.displays[1].Activate();

            // Set the secondary camera to render to the second display
            secondaryCamera.targetDisplay = 1;
            secondaryCanvas.worldCamera = secondaryCamera;

            // Note: The second display's settings are automatically determined when it activates.
        }
    }
}
