using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SpinRush.Audio;
using SpinRush.Core;
using SpinRush.Effects;
using SpinRush.Gameplay;
using SpinRush.UI;

namespace SpinRush.Editor
{
    /// <summary>
    /// Editor utility to construct the complete visual hierarchy, prefabs, and main game scene.
    /// Aligns canvas elements to exact analyzed pixel geometry with Royal VIP Indian Rupee (₹) styling,
    /// modal dialogs, procedural audio synthesis, and visual particle celebration effects.
    /// </summary>
    public static class SceneSetupEditor
    {
        [MenuItem("SpinRush/Build Complete Game Scene & Prefabs")]
        public static void BuildSceneAndPrefabs()
        {
            Debug.Log("[SpinRush Scene Setup] Building Royal VIP game scene and prefabs...");

            EnsureFolderExists("Assets/Prefabs");
            EnsureFolderExists("Assets/Scenes");

            // 1. Create / Update Reel Prefab
            GameObject reelPrefabObj = CreateReelPrefab();

            // 2. Create and configure MainGameScene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera & AudioListener
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.03f, 0.12f);
            cam.orthographic = true;
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(0, 0, -10);

            // EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            // Canvas (1920x1080 Reference, Match 0.5)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/bg_gradient.png");

            // Slot Machine Root Container (Scaled 1.32x for commanding VIP screen presence)
            GameObject machineRoot = new GameObject("SlotMachineRoot", typeof(RectTransform));
            machineRoot.transform.SetParent(canvasObj.transform, false);
            RectTransform machineRect = machineRoot.GetComponent<RectTransform>();
            machineRect.anchorMin = new Vector2(0.5f, 0.5f);
            machineRect.anchorMax = new Vector2(0.5f, 0.5f);
            machineRect.pivot = new Vector2(0.5f, 0.5f);
            machineRect.anchoredPosition = new Vector2(0f, 60f);
            machineRect.sizeDelta = new Vector2(816f, 624f);
            machineRect.localScale = new Vector3(1.32f, 1.32f, 1f);

            // 1. Reels Masked Viewport (Placed behind machine cabinet)
            // Cutout bounds in 816x624 texture: X 232..593 (width 361), Y 250..451 (height 201)
            // Local pos relative to center (408, 312): Center X = 4.5f, Center Y = 38.5f
            GameObject viewportObj = new GameObject("ReelsViewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObj.transform.SetParent(machineRoot.transform, false);
            RectTransform vpRect = viewportObj.GetComponent<RectTransform>();
            vpRect.anchorMin = new Vector2(0.5f, 0.5f);
            vpRect.anchorMax = new Vector2(0.5f, 0.5f);
            vpRect.pivot = new Vector2(0.5f, 0.5f);
            vpRect.anchoredPosition = new Vector2(4.5f, 38.5f);
            vpRect.sizeDelta = new Vector2(362f, 202f);

            // Instantiate 3 Reels inside Viewport
            SymbolDatabase db = AssetDatabase.LoadAssetAtPath<SymbolDatabase>("Assets/Data/SymbolDatabase.asset");
            float[] reelXOffsets = new float[] { -120f, 0f, 120f };
            var reelList = new List<SlotReel>();

            for (int i = 0; i < 3; i++)
            {
                GameObject reelInstance = (GameObject)PrefabUtility.InstantiatePrefab(reelPrefabObj, viewportObj.transform);
                reelInstance.name = $"Reel_{i + 1}";
                RectTransform rRect = reelInstance.GetComponent<RectTransform>();
                rRect.anchoredPosition = new Vector2(reelXOffsets[i], 0f);
                SlotReel reelComp = reelInstance.GetComponent<SlotReel>();
                reelComp.Initialize(i, db);
                reelList.Add(reelComp);
            }

            // 2. Cabinet Body (slot-machine4.png with transparent cutout)
            GameObject cabinetObj = new GameObject("CabinetBody", typeof(RectTransform), typeof(Image));
            cabinetObj.transform.SetParent(machineRoot.transform, false);
            RectTransform cabRect = cabinetObj.GetComponent<RectTransform>();
            cabRect.anchorMin = Vector2.zero;
            cabRect.anchorMax = Vector2.one;
            cabRect.sizeDelta = Vector2.zero;
            Image cabImg = cabinetObj.GetComponent<Image>();
            cabImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine4.png");
            cabImg.raycastTarget = false;

            // 3. Glass Overlay & Dividers (slot-machine5.png)
            GameObject glassObj = new GameObject("GlassOverlay", typeof(RectTransform), typeof(Image));
            glassObj.transform.SetParent(machineRoot.transform, false);
            RectTransform glassRect = glassObj.GetComponent<RectTransform>();
            glassRect.anchorMin = Vector2.zero;
            glassRect.anchorMax = Vector2.one;
            glassRect.sizeDelta = Vector2.zero;
            Image glassImg = glassObj.GetComponent<Image>();
            glassImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine5.png");
            glassImg.raycastTarget = false;

            // 4. Interactive Lever (slot-machine2.png / slot-machine3.png)
            Sprite leverUp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine2.png");
            Sprite leverDown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine3.png");

            GameObject leverObj = new GameObject("Lever", typeof(RectTransform), typeof(Image), typeof(LeverController));
            leverObj.transform.SetParent(machineRoot.transform, false);
            RectTransform leverRect = leverObj.GetComponent<RectTransform>();
            leverRect.anchorMin = new Vector2(0.5f, 0.5f);
            leverRect.anchorMax = new Vector2(0.5f, 0.5f);
            leverRect.pivot = new Vector2(0.5f, 0.5f);
            leverRect.anchoredPosition = new Vector2(435f, 10f);
            leverRect.sizeDelta = new Vector2(110f, 260f);

            Image leverImg = leverObj.GetComponent<Image>();
            leverImg.sprite = leverUp;
            leverImg.preserveAspect = true;

            // 5. HUD Dashboard / Middle Box (slot_machine_Middle_box.png)
            GameObject hudObj = new GameObject("HUDPanel", typeof(RectTransform), typeof(Image), typeof(MiddleBoxHUD));
            hudObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hudRect = hudObj.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 0.5f);
            hudRect.anchorMax = new Vector2(0.5f, 0.5f);
            hudRect.pivot = new Vector2(0.5f, 0.5f);
            hudRect.anchoredPosition = new Vector2(0f, -400f);
            hudRect.sizeDelta = new Vector2(820f, 140f);
            Image hudImg = hudObj.GetComponent<Image>();
            hudImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot_machine_Middle_box.png");

            // Add Balance, Bet, Win Text Labels inside HUD
            Text balText = CreateHUDLabel(hudObj.transform, "BalanceSection", "BALANCE", "₹1,00,000", new Vector2(-260f, 0f), new Color(1f, 0.85f, 0.3f));
            Text betText = CreateHUDLabel(hudObj.transform, "BetSection", "VIP BET", "₹500", new Vector2(0f, 0f), Color.white);
            Text winText = CreateHUDLabel(hudObj.transform, "WinSection", "LAST WIN", "₹0", new Vector2(260f, 0f), new Color(0.35f, 1f, 0.55f));

            // 6. Buttons (Bet -, Bet +, Spin)
            GameObject btnMinusObj = CreateButton(hudObj.transform, "Btn_BetMinus", new Vector2(-80f, 0f), new Vector2(56f, 56f), "Assets/slot_machine_buttons-03.png", "btn_bet_minus");
            GameObject btnPlusObj = CreateButton(hudObj.transform, "Btn_BetPlus", new Vector2(80f, 0f), new Vector2(56f, 56f), "Assets/slot_machine_buttons-04.png", "btn_bet_plus");
            GameObject spinBtnObj = CreateButton(canvasObj.transform, "Btn_Spin", new Vector2(530f, -400f), new Vector2(130f, 130f), "Assets/slot_machine_buttons-02.png", "btn_spin");

            // 7. Particle System & Visual Effects Presenter
            ParticleSystem goldParticles = CreateParticleSystem(canvasObj.transform);
            var fxComp = machineRoot.AddComponent<WinEffectsPresenter>();
            fxComp.Initialize(machineRect, goldParticles);

            // 8. Procedural Audio Engine
            var audioComp = machineRoot.AddComponent<AudioController>();

            // 9. Modal Popup Layer (popup.png, Yes_No_Btn.png)
            WinPopupController popupController = CreateModalPopup(canvasObj.transform);

            // Attach Core Controllers to machineRoot
            var rngComp = machineRoot.AddComponent<RandomNumberGenerator>();
            var walletComp = machineRoot.AddComponent<WalletManager>();
            var controller = machineRoot.AddComponent<SlotMachineController>();
            controller.Configure(db, rngComp, walletComp, reelList, popupController, audioComp, fxComp);

            LeverController leverCtrl = leverObj.GetComponent<LeverController>();
            leverCtrl.Initialize(leverUp, leverDown, controller);

            MiddleBoxHUD hudComp = hudObj.GetComponent<MiddleBoxHUD>();
            hudComp.Initialize(balText, betText, winText, walletComp, controller);

            Button btnMinus = btnMinusObj.GetComponent<Button>();
            if (btnMinus != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMinus.onClick, walletComp.DecreaseBet);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMinus.onClick, audioComp.PlayButtonClick);
            }

            Button btnPlus = btnPlusObj.GetComponent<Button>();
            if (btnPlus != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlus.onClick, walletComp.IncreaseBet);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlus.onClick, audioComp.PlayButtonClick);
            }

            Button spinBtn = spinBtnObj.GetComponent<Button>();
            if (spinBtn != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(spinBtn.onClick, controller.OnSpinButtonClicked);
            }

            // Save scene
            string scenePath = "Assets/Scenes/MainGameScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in EditorBuildSettings
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            Debug.Log($"[SpinRush Scene Setup] Successfully constructed and saved complete game scene at: {scenePath}");
        }

        private static ParticleSystem CreateParticleSystem(Transform parent)
        {
            GameObject psObj = new GameObject("CelebrationParticles", typeof(RectTransform), typeof(ParticleSystem));
            psObj.transform.SetParent(parent, false);
            RectTransform psRect = psObj.GetComponent<RectTransform>();
            psRect.anchoredPosition = new Vector2(0f, 60f);

            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.2f;
            main.startLifetime = 1.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(200f, 600f);
            main.startSize = new ParticleSystem.MinMaxCurve(16f, 32f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.2f), new Color(1f, 0.65f, 0.1f));
            main.gravityModifier = 1.5f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 80f;

            return ps;
        }

        private static WinPopupController CreateModalPopup(Transform parent)
        {
            // Modal Root
            GameObject modalRoot = new GameObject("ModalLayer", typeof(RectTransform), typeof(WinPopupController));
            modalRoot.transform.SetParent(parent, false);
            RectTransform rootRect = modalRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // Dark Backdrop
            GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(modalRoot.transform, false);
            RectTransform bdRect = backdrop.GetComponent<RectTransform>();
            bdRect.anchorMin = Vector2.zero;
            bdRect.anchorMax = Vector2.one;
            bdRect.sizeDelta = Vector2.zero;
            Image bdImg = backdrop.GetComponent<Image>();
            bdImg.color = new Color(0f, 0f, 0f, 0.78f);

            // Modal Container
            GameObject container = new GameObject("ModalContainer", typeof(RectTransform), typeof(Image));
            container.transform.SetParent(modalRoot.transform, false);
            RectTransform cRect = container.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(720f, 440f);
            Image cImg = container.GetComponent<Image>();
            cImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/popup.png");
            cImg.preserveAspect = true;

            // Title Text
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(container.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 110f);
            titleRect.sizeDelta = new Vector2(620f, 50f);
            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.fontSize = 26;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.9f, 0.4f);

            // Amount Text
            GameObject amountObj = new GameObject("AmountText", typeof(RectTransform), typeof(Text));
            amountObj.transform.SetParent(container.transform, false);
            RectTransform amountRect = amountObj.GetComponent<RectTransform>();
            amountRect.anchoredPosition = new Vector2(0f, 45f);
            amountRect.sizeDelta = new Vector2(620f, 60f);
            Text amountTxt = amountObj.GetComponent<Text>();
            amountTxt.fontSize = 40;
            amountTxt.fontStyle = FontStyle.Bold;
            amountTxt.alignment = TextAnchor.MiddleCenter;
            amountTxt.color = new Color(0.35f, 1f, 0.55f);

            // Message Text
            GameObject msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(Text));
            msgObj.transform.SetParent(container.transform, false);
            RectTransform msgRect = msgObj.GetComponent<RectTransform>();
            msgRect.anchoredPosition = new Vector2(0f, -25f);
            msgRect.sizeDelta = new Vector2(600f, 60f);
            Text msgTxt = msgObj.GetComponent<Text>();
            msgTxt.fontSize = 20;
            msgTxt.fontStyle = FontStyle.Normal;
            msgTxt.alignment = TextAnchor.MiddleCenter;
            msgTxt.color = Color.white;

            // Buttons (Yes & No)
            GameObject yesBtnObj = CreateButton(container.transform, "Btn_Yes", new Vector2(-110f, -115f), new Vector2(170f, 62f), "Assets/Yes_No_Btn.png", "btn_yes");
            GameObject noBtnObj = CreateButton(container.transform, "Btn_No", new Vector2(110f, -115f), new Vector2(170f, 62f), "Assets/Yes_No_Btn.png", "btn_no");

            WinPopupController ctrl = modalRoot.GetComponent<WinPopupController>();
            ctrl.Initialize(backdrop, cRect, titleTxt, msgTxt, amountTxt, yesBtnObj.GetComponent<Button>(), noBtnObj.GetComponent<Button>());

            return ctrl;
        }

        private static GameObject CreateReelPrefab()
        {
            GameObject reelRoot = new GameObject("SlotReel", typeof(RectTransform), typeof(SlotReel));
            RectTransform rootRect = reelRoot.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(110f, 200f);

            GameObject symbolsContainer = new GameObject("SymbolsContainer", typeof(RectTransform));
            symbolsContainer.transform.SetParent(reelRoot.transform, false);
            RectTransform scRect = symbolsContainer.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.5f, 0.5f);
            scRect.anchorMax = new Vector2(0.5f, 0.5f);
            scRect.sizeDelta = new Vector2(110f, 500f);

            // Create 5 symbol slots (-200, -100, 0, 100, 200)
            float[] yPositions = new float[] { 200f, 100f, 0f, -100f, -200f };
            for (int i = 0; i < 5; i++)
            {
                GameObject symObj = new GameObject($"SlotSymbol_{i}", typeof(RectTransform), typeof(Image), typeof(SlotSymbol));
                symObj.transform.SetParent(symbolsContainer.transform, false);
                RectTransform symRect = symObj.GetComponent<RectTransform>();
                symRect.anchorMin = new Vector2(0.5f, 0.5f);
                symRect.anchorMax = new Vector2(0.5f, 0.5f);
                symRect.pivot = new Vector2(0.5f, 0.5f);
                symRect.anchoredPosition = new Vector2(0f, yPositions[i]);
                symRect.sizeDelta = new Vector2(92f, 92f);

                Image symImg = symObj.GetComponent<Image>();
                symImg.preserveAspect = true;
            }

            string prefabPath = "Assets/Prefabs/ReelPrefab.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(reelRoot, prefabPath);
            GameObject.DestroyImmediate(reelRoot);
            Debug.Log($"[SpinRush Scene Setup] Created ReelPrefab at: {prefabPath}");
            return savedPrefab;
        }

        private static Text CreateHUDLabel(Transform parent, string name, string labelText, string defaultValue, Vector2 pos, Color valueColor)
        {
            GameObject secObj = new GameObject(name, typeof(RectTransform));
            secObj.transform.SetParent(parent, false);
            RectTransform secRect = secObj.GetComponent<RectTransform>();
            secRect.anchoredPosition = pos;
            secRect.sizeDelta = new Vector2(200f, 100f);

            // Title Label
            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblObj.transform.SetParent(secObj.transform, false);
            RectTransform lblRect = lblObj.GetComponent<RectTransform>();
            lblRect.anchoredPosition = new Vector2(0f, 22f);
            lblRect.sizeDelta = new Vector2(180f, 30f);
            Text lblText = lblObj.GetComponent<Text>();
            lblText.text = labelText;
            lblText.fontSize = 18;
            lblText.fontStyle = FontStyle.Bold;
            lblText.alignment = TextAnchor.MiddleCenter;
            lblText.color = new Color(0.95f, 0.85f, 0.6f);

            // Value Display
            GameObject valObj = new GameObject("ValueText", typeof(RectTransform), typeof(Text));
            valObj.transform.SetParent(secObj.transform, false);
            RectTransform valRect = valObj.GetComponent<RectTransform>();
            valRect.anchoredPosition = new Vector2(0f, -15f);
            valRect.sizeDelta = new Vector2(180f, 40f);
            Text valText = valObj.GetComponent<Text>();
            valText.text = defaultValue;
            valText.fontSize = 28;
            valText.fontStyle = FontStyle.Bold;
            valText.alignment = TextAnchor.MiddleCenter;
            valText.color = valueColor;

            return valText;
        }

        private static GameObject CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, string spriteSheetPath, string prefix)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            Button btn = btnObj.GetComponent<Button>();

            Sprite[] sprites = LoadAllSpritesFromPath(spriteSheetPath);
            Sprite normal = FindSpriteByName(sprites, $"{prefix}_normal");
            Sprite highlighted = FindSpriteByName(sprites, $"{prefix}_highlighted");
            Sprite pressed = FindSpriteByName(sprites, $"{prefix}_pressed");
            Sprite disabled = FindSpriteByName(sprites, $"{prefix}_disabled");

            if (normal == null && sprites != null && sprites.Length > 0) normal = sprites[0];

            if (normal != null) img.sprite = normal;

            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState state = new SpriteState
            {
                highlightedSprite = highlighted != null ? highlighted : normal,
                pressedSprite = pressed != null ? pressed : normal,
                disabledSprite = disabled != null ? disabled : normal
            };
            btn.spriteState = state;

            return btnObj;
        }

        private static Sprite[] LoadAllSpritesFromPath(string path)
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
            var list = new List<Sprite>();
            foreach (var o in objs)
            {
                if (o is Sprite s) list.Add(s);
            }
            return list.ToArray();
        }

        private static Sprite FindSpriteByName(Sprite[] sprites, string name)
        {
            if (sprites == null) return null;
            foreach (var s in sprites)
            {
                if (s != null && s.name == name) return s;
            }
            return null;
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
