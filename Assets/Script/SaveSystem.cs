using UnityEngine;

public static class SaveSystem
{
    // Elmenti az adott slot adatait
    public static void SaveGame(int slotIndex, int levelIndex, int difficulty, int round)
    {
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Exists", 1); // Jelzi, hogy ez a slot foglalt
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Level", levelIndex);
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Difficulty", difficulty);
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Round", round);
        PlayerPrefs.Save();
        Debug.Log($"Game Saved to Slot {slotIndex}: Level {levelIndex}, Diff {difficulty}");
    }

    // Törli a mentést (pl. Hard módban halálkor, vagy menübõl)
    public static void DeleteSave(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Difficulty");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Round");
        PlayerPrefs.Save();
        Debug.Log($"Save Slot {slotIndex} deleted.");
    }

    // Megnézi, van-e mentés az adott sloton
    public static bool HasSave(int slotIndex)
    {
        return PlayerPrefs.HasKey($"Slot_{slotIndex}_Exists");
    }

    // Segédfüggvények az adatok lekéréséhez
    public static int GetSavedLevel(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Level", 1); // Default Level 1
    public static int GetSavedDifficulty(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Difficulty", 1);
    public static int GetSavedRound(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Round", 1);
}