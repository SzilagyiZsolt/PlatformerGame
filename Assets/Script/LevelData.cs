using UnityEngine;

[System.Serializable] // Ez teszi láthatóvá az Inspectorban
public class LevelData
{
    public string levelName;      // Pl. "Level 1"
    public int sceneIndex;        // A Build Settings-es szám (pl. 1)
    public Sprite levelImage;     // Kép a pályáról
}