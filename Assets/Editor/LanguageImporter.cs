using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using Assets.Scripts.Collectables; // Ensure this matches your WordData namespace

public class LanguageImporter
{
    // Path to your CSV file
    private static string csvPath = Application.dataPath + "/ExternalData/LanguageData.csv";
    // Path where the ScriptableObject asset is stored
    private static string assetPath = "Assets/Resources/MasterLanguageDB.asset";

    [MenuItem("Tools/Sync Language CSV")]
    public static void Sync()
    {
        // Load or Create the Master Database Asset
        LanguageDatabase db = AssetDatabase.LoadAssetAtPath<LanguageDatabase>(assetPath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LanguageDatabase>();
            AssetDatabase.CreateAsset(db, assetPath);
            Debug.Log("<color=cyan>Created new MasterLanguageDB asset.</color>");
        }

        // Clear existing entries to prevent duplicates
        db.allEntries.Clear();

        // Read the CSV file
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found at: {csvPath}. Please check your 'ExternalData' folder.");
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        // Parse the lines (starting at index 1 to skip the header row)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Using a simple split - note: if your text contains commas, 
            // you'll eventually need a more robust CSV parser.
            string[] c = line.Split(',');

            // SAFETY: We now expect 16 columns (Index 0 to 15)
            if (c.Length < 16)
            {
                Debug.LogWarning($"Line {i} only has {c.Length} columns. Needs 16. Skipping.");
                continue;
            }

            try
            {
                // Create the WordData using the updated constructor
                // Ensure WordData.cs constructor matches this order
                WordData entry = new WordData(
                    c[0].Trim(),                               // key
                    c[1].Trim(),                               // language
                    c[2].Trim(),                               // english
                    c[3].Trim(),                               // complex (Hangul)
                    c[4].Trim(),                               // romanized
                    c[5].Trim(),                               // phonetic
                    ParseEnum<WordData.WordDataType>(c[6]),    // wordDataType
                    ParseEnum<WordData.PartOfSpeech>(c[7]),    // partOfSpeech
                    ParseEnum<WordData.Gender>(c[8]),          // gender
                    ParseEnum<WordData.Tense>(c[9]),           // tense
                    ParseEnum<WordData.Formality>(c[10]),      // formality
                    c[11].Trim(),                              // category
                    c[12].Trim(),                              // subCategory
                    ParseInt(c[13]),                           // unit
                    ParseInt(c[14]),                           // lesson
                    c[15].Trim()                               // voiceId / AudioKey
                );

                db.allEntries.Add(entry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing line {i}: {ex.Message}");
            }
        }

        // Save the Asset
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Successfully imported {db.allEntries.Count} entries into MasterLanguageDB!</color>");
    }

    private static T ParseEnum<T>(string value) where T : struct
    {
        // Clean the string immediately
        string cleanValue = value?.Trim();

        // Handle empty cells or "NA" strings by trying to find "NA" in your Enum
        if (string.IsNullOrEmpty(cleanValue) || cleanValue.Equals("NA", StringComparison.OrdinalIgnoreCase))
        {
            // Try to see if the Enum actually HAS a member named "NA"
            if (Enum.TryParse("NA", true, out T naResult))
            {
                return naResult;
            }
            return default(T);
        }

        // Try to parse the actual value (e.g., "Verb", "Polite")
        if (Enum.TryParse(cleanValue, true, out T result))
        {
            return result;
        }

        return default(T);
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out int result)) return result;
        return 0;
    }
}