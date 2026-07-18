using UnityEngine;

public class ResolutionSetup : MonoBehaviour
{


    private void Start()
    {
        Resolution resolution = Screen.currentResolution;
        Screen.SetResolution(
            resolution.width,
            resolution.height,
            FullScreenMode.FullScreenWindow
        );
    }
    //void Start()
    //{
    //    Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    //}
}