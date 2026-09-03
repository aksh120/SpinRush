using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SpinRush.Gameplay;

namespace SpinRush.Editor
{
    /// <summary>
    /// Automated asset processing pipeline for SpinRush.
    /// Configures texture importer settings, generates sprite slices, and creates ScriptableObject databases.
    /// </summary>
    public static class AutoSpriteSlicer
    {
        [MenuItem("SpinRush/Process All Assets & Build Database")]
        public static void ProcessAllAssets()
        {
            Debug.Log("[SpinRush Asset Pipeline] Starting automated texture slicing and database configuration...");

            // 1. Configure Single Sprites
            ConfigureSingleSprite("Assets/bg_gradient.png");
            ConfigureSingleSprite("Assets/slot-machine1.png");
            ConfigureSingleSprite("Assets/slot-machine2.png");
            ConfigureSingleSprite("Assets/slot-machine3.png");
            ConfigureSingleSprite("Assets/slot-machine4.png");
            ConfigureSingleSprite("Assets/slot-machine5.png");
            ConfigureSingleSprite("Assets/slot-symbol1.png");
            ConfigureSingleSprite("Assets/slot-symbol2.png");
            ConfigureSingleSprite("Assets/slot-symbol3.png");
            ConfigureSingleSprite("Assets/slot-symbol4.png");
            ConfigureSingleSprite("Assets/slot_machine_Middle_box.png");
            ConfigureSingleSprite("Assets/popup.png");
            ConfigureSingleSprite("Assets/lever_arm_isolated.png");

            // 2. Configure Multi-Sprite Sheets (Buttons)
            SliceVerticalButtonSheet("Assets/slot_machine_buttons-02.png", "btn_spin");
            SliceVerticalButtonSheet("Assets/slot_machine_buttons-03.png", "btn_bet_minus");
            SliceVerticalButtonSheet("Assets/slot_machine_buttons-04.png", "btn_bet_plus");
            SliceYesNoButtonSheet("Assets/Yes_No_Btn.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 3. Build Symbol ScriptableObjects & Database
            BuildSymbolDatabase();

            Debug.Log("[SpinRush Asset Pipeline] Asset processing & database construction complete!");
        }

        private static void ConfigureSingleSprite(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"[SpinRush] Asset not found at path: {assetPath}");
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static void SliceVerticalButtonSheet(string assetPath, string prefix)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"[SpinRush] Button sheet not found at path: {assetPath}");
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;

                // 4 cells: 256x256 each (Total height = 1024)
                // Unity Y: 0 is bottom.
                // Top row (Normal) -> Y: 768..1024
                // Row 2 (Highlighted) -> Y: 512..768
                // Row 3 (Pressed) -> Y: 256..512
                // Row 4 (Disabled) -> Y: 0..256

                var metas = new List<SpriteMetaData>
                {
                    new SpriteMetaData
                    {
                        name = $"{prefix}_normal",
                        rect = new Rect(0, 768, 256, 256),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    },
                    new SpriteMetaData
                    {
                        name = $"{prefix}_highlighted",
                        rect = new Rect(0, 512, 256, 256),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    },
                    new SpriteMetaData
                    {
                        name = $"{prefix}_pressed",
                        rect = new Rect(0, 256, 256, 256),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    },
                    new SpriteMetaData
                    {
                        name = $"{prefix}_disabled",
                        rect = new Rect(0, 0, 256, 256),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    }
                };

                importer.spritesheet = metas.ToArray();
                importer.SaveAndReimport();
            }
        }

        private static void SliceYesNoButtonSheet(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"[SpinRush] Yes/No button sheet not found at path: {assetPath}");
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;

                // Texture is 988 x 689
                // Col 0 (Yes): X = 60..450 (width 390)
                // Col 1 (No): X = 538..928 (width 390)
                // Row 0 (Top / Normal): Y = 465..605 (height 140)
                // Row 1 (Mid / Hover): Y = 274..414 (height 140)
                // Row 2 (Bottom / Pressed): Y = 83..223 (height 140)

                var metas = new List<SpriteMetaData>
                {
                    // Yes Buttons
                    new SpriteMetaData { name = "btn_yes_normal", rect = new Rect(60, 465, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
                    new SpriteMetaData { name = "btn_yes_highlighted", rect = new Rect(60, 274, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
                    new SpriteMetaData { name = "btn_yes_pressed", rect = new Rect(60, 83, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },

                    // No Buttons
                    new SpriteMetaData { name = "btn_no_normal", rect = new Rect(538, 465, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
                    new SpriteMetaData { name = "btn_no_highlighted", rect = new Rect(538, 274, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
                    new SpriteMetaData { name = "btn_no_pressed", rect = new Rect(538, 83, 390, 140), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) }
                };

                importer.spritesheet = metas.ToArray();
                importer.SaveAndReimport();
            }
        }

        private static void BuildSymbolDatabase()
        {
            EnsureFolderExists("Assets/Data");
            EnsureFolderExists("Assets/Data/Symbols");

            // Load Symbol Sprites
            Sprite sym1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-symbol1.png");
            Sprite sym2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-symbol2.png");
            Sprite sym3Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-symbol3.png");
            Sprite sym4Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-symbol4.png");

            var symList = new List<SymbolData>();

            // SYM_01: Lucky Seven (Red) - Top Tier (50x)
            var sym1 = CreateOrUpdateSymbolData("Assets/Data/Symbols/Symbol_01_LuckySeven.asset", "SYM_01", "Lucky Seven", sym1Sprite, 50, 1, false);
            symList.Add(sym1);

            // SYM_02: Golden Bell - High Tier (25x)
            var sym2 = CreateOrUpdateSymbolData("Assets/Data/Symbols/Symbol_02_GoldenBell.asset", "SYM_02", "Golden Bell", sym2Sprite, 25, 2, false);
            symList.Add(sym2);

            // SYM_04: Triple Bar (Blue) - Base Tier (10x)
            var sym4 = CreateOrUpdateSymbolData("Assets/Data/Symbols/Symbol_04_TripleBar.asset", "SYM_04", "Triple Bar", sym4Sprite, 10, 4, false);
            symList.Add(sym4);

            // SYM_03: Golden Star / Wild - Bonus Tier (100x on 3-Wild, 2x Wild multiplier)
            var sym3 = CreateOrUpdateSymbolData("Assets/Data/Symbols/Symbol_03_StarWild.asset", "SYM_03", "Star Wild", sym3Sprite, 100, 3, true, 2.0f);
            symList.Add(sym3);

            // Create or update SymbolDatabase.asset
            string dbPath = "Assets/Data/SymbolDatabase.asset";
            SymbolDatabase db = AssetDatabase.LoadAssetAtPath<SymbolDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<SymbolDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            db.SetSymbols(symList);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SpinRush Asset Pipeline] Symbol database populated with {symList.Count} symbols at {dbPath}.");
        }

        private static SymbolData CreateOrUpdateSymbolData(string path, string id, string name, Sprite icon, int payout, int tier, bool isWild, float wildMult = 2f)
        {
            SymbolData data = AssetDatabase.LoadAssetAtPath<SymbolData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SymbolData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.Initialize(id, name, icon, payout, tier, isWild, wildMult);
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
                string folderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
