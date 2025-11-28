using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    // Ezt a függvényt hívjuk meg a gombokról
    public void Click()
    {
        // Megkeressük az EGYETLEN létezõ AudioManagert, és szólunk neki
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonSound();
        }
    }
}