using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    // --- MÁR MEGLÉVÕ MENTÉSI LOGIKA ---

    public static void SaveGame(int slotIndex, int levelIndex, int difficulty, int round)
    {
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Exists", 1);
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Level", levelIndex);
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Difficulty", difficulty);
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Round", round);
        PlayerPrefs.Save();
        Debug.Log($"Game Saved to Slot {slotIndex}: Level {levelIndex}, Diff {difficulty}");
    }

    public static void DeleteSave(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Difficulty");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Round");
        PlayerPrefs.Save();
        Debug.Log($"Save Slot {slotIndex} deleted.");
    }

    public static bool HasSave(int slotIndex)
    {
        return PlayerPrefs.HasKey($"Slot_{slotIndex}_Exists");
    }

    public static int GetSavedLevel(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Level", 1);
    public static int GetSavedDifficulty(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Difficulty", 1);
    public static int GetSavedRound(int slotIndex) => PlayerPrefs.GetInt($"Slot_{slotIndex}_Round", 1);


    // --- ÚJ RÉSZ: CSAPDÁK MENTÉSE A 10. PÁLYÁHOZ ---

    // Egy segédosztály, mert a JsonUtility nem tud közvetlenül List<Vector3>-at menteni
    [System.Serializable]
    public class TrapListWrapper
    {
        public List<Vector3> positions;
    }

    // Elmenti az adott pályához tartozó összes tüskét
    public static void SaveTrapsForLevel(int levelIndex, List<Vector3> traps)
    {
        TrapListWrapper wrapper = new TrapListWrapper { positions = traps };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString($"Level_{levelIndex}_Traps", json);
        PlayerPrefs.Save();
    }

    // Betölti az adott pályához tartozó tüskéket (a 10. pályán használjuk)
    public static List<Vector3> LoadTrapsForLevel(int levelIndex)
    {
        string key = $"Level_{levelIndex}_Traps";
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            TrapListWrapper wrapper = JsonUtility.FromJson<TrapListWrapper>(json);
            return wrapper.positions;
        }
        return new List<Vector3>(); // Ha nincs mentés, üres listát adunk vissza
    }
}