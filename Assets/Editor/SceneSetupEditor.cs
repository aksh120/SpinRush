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
    /// Master Editor utility to construct the complete visual hierarchy, prefabs, and main game scene.
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

            SymbolDatabase db = AssetDatabase.LoadAssetAtPath<SymbolDatabase>("Assets/Data/SymbolDatabase.asset");

            // 1. Create / Update Reel Prefab with 20 rich symbol tiles
            GameObject reelPrefabObj = CreateReelPrefab(db);

            // 2. Create and configure MainGameScene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera & AudioListener
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.02f, 0.10f);
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

            // Slot Machine Root Container (Scaled 1.35x for commanding VIP screen presence)
            GameObject machineRoot = new GameObject("SlotMachineRoot", typeof(RectTransform));
            machineRoot.transform.SetParent(canvasObj.transform, false);
            RectTransform machineRect = machineRoot.GetComponent<RectTransform>();
            machineRect.anchorMin = new Vector2(0.5f, 0.5f);
            machineRect.anchorMax = new Vector2(0.5f, 0.5f);
            machineRect.pivot = new Vector2(0.5f, 0.5f);
            machineRect.anchoredPosition = new Vector2(0f, 75f);
            machineRect.sizeDelta = new Vector2(816f, 624f);
            machineRect.localScale = new Vector3(1.35f, 1.35f, 1f);

            // ==========================================
            // LAYER 1: REELS BACKING (slot-machine5.png BEHIND REELS)
            // ==========================================
            GameObject backingObj = new GameObject("ReelsBacking", typeof(RectTransform), typeof(Image));
            backingObj.transform.SetParent(machineRoot.transform, false);
            RectTransform backingRect = backingObj.GetComponent<RectTransform>();
            backingRect.anchorMin = Vector2.zero;
            backingRect.anchorMax = Vector2.one;
            backingRect.sizeDelta = Vector2.zero;
            Image backingImg = backingObj.GetComponent<Image>();
            backingImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine5.png");
            backingImg.raycastTarget = false;

            // ==========================================
            // LAYER 2: REELS VIEWPORT (CONTAINING 3 REELS)
            // Exact local center in 816x624: X = +4.5px, Y = -38.5px
            // ==========================================
            GameObject viewportObj = new GameObject("ReelsViewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObj.transform.SetParent(machineRoot.transform, false);
            RectTransform vpRect = viewportObj.GetComponent<RectTransform>();
            vpRect.anchorMin = new Vector2(0.5f, 0.5f);
            vpRect.anchorMax = new Vector2(0.5f, 0.5f);
            vpRect.pivot = new Vector2(0.5f, 0.5f);
            vpRect.anchoredPosition = new Vector2(4.5f, -38.5f);
            vpRect.sizeDelta = new Vector2(368f, 208f);

            // Instantiate 3 Reels inside Viewport
            // Exact horizontal centers of the 3 cutouts in 816x624:
            // Cutout 1: Local X = -125.5px -> relative to Viewport (X=4.5): -130px
            // Cutout 2: Local X = +4.5px   -> relative to Viewport (X=4.5): 0px
            // Cutout 3: Local X = +134.5px -> relative to Viewport (X=4.5): +130px
            float[] reelXOffsets = new float[] { -130f, 0f, 130f };
            var reelList = new List<SlotReel>();

            for (int i = 0; i < 3; i++)
            {
                GameObject reelInstance = (GameObject)PrefabUtility.InstantiatePrefab(reelPrefabObj, viewportObj.transform);
                reelInstance.name = $"Reel_{i + 1}";
                RectTransform rRect = reelInstance.GetComponent<RectTransform>();
                rRect.anchorMin = new Vector2(0.5f, 0.5f);
                rRect.anchorMax = new Vector2(0.5f, 0.5f);
                rRect.pivot = new Vector2(0.5f, 0.5f);
                rRect.anchoredPosition = new Vector2(reelXOffsets[i], 0f);
                rRect.sizeDelta = new Vector2(104f, 208f);

                // Add RectMask2D to each reel column to strictly clip symbols inside their 104px slit
                if (reelInstance.GetComponent<RectMask2D>() == null)
                {
                    reelInstance.AddComponent<RectMask2D>();
                }

                SlotReel reelComp = reelInstance.GetComponent<SlotReel>();
                reelComp.Initialize(i, db);
                reelList.Add(reelComp);
            }

            // ==========================================
            // LAYER 3: CABINET BODY (slot-machine4.png with transparent cutout)
            // ==========================================
            GameObject cabinetObj = new GameObject("CabinetBody", typeof(RectTransform), typeof(Image));
            cabinetObj.transform.SetParent(machineRoot.transform, false);
            RectTransform cabRect = cabinetObj.GetComponent<RectTransform>();
            cabRect.anchorMin = Vector2.zero;
            cabRect.anchorMax = Vector2.one;
            cabRect.sizeDelta = Vector2.zero;
            Image cabImg = cabinetObj.GetComponent<Image>();
            cabImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine4.png");
            cabImg.raycastTarget = false;

            // ==========================================
            // LAYER 4: REALISTIC PHYSICAL LEVER (Rotating Arm & Hinge Pivot)
            // ==========================================
            Sprite leverArmSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/lever_arm_isolated.png");
            if (leverArmSprite == null)
            {
                leverArmSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine2.png");
            }
            Sprite leverDownOverlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot-machine3.png");

            // Isolated rotating lever arm (Pivot at bottom center of rod: hinge point 310.5, -253)
            GameObject leverArmObj = new GameObject("LeverArm", typeof(RectTransform), typeof(Image));
            leverArmObj.transform.SetParent(machineRoot.transform, false);
            RectTransform armRect = leverArmObj.GetComponent<RectTransform>();
            armRect.anchorMin = new Vector2(0.5f, 0.5f);
            armRect.anchorMax = new Vector2(0.5f, 0.5f);
            armRect.pivot = new Vector2(0.5f, 0f); // Pivot at bottom of metallic rod!
            armRect.anchoredPosition = new Vector2(310.5f, -253f);
            armRect.sizeDelta = new Vector2(94f, 270f);
            Image armImg = leverArmObj.GetComponent<Image>();
            armImg.sprite = leverArmSprite;
            armImg.preserveAspect = true;
            armImg.raycastTarget = false;

            // Down overlay frame (slot-machine3.png)
            GameObject leverDownObj = new GameObject("LeverDownOverlay", typeof(RectTransform), typeof(Image));
            leverDownObj.transform.SetParent(machineRoot.transform, false);
            RectTransform downRect = leverDownObj.GetComponent<RectTransform>();
            downRect.anchorMin = Vector2.zero;
            downRect.anchorMax = Vector2.one;
            downRect.sizeDelta = Vector2.zero;
            Image downImg = leverDownObj.GetComponent<Image>();
            downImg.sprite = leverDownOverlaySprite;
            downImg.raycastTarget = false;
            leverDownObj.SetActive(false);

            // Lever Click / Drag Hit Area (covers full lever range on right)
            GameObject leverHitObj = new GameObject("LeverHitArea", typeof(RectTransform), typeof(Image), typeof(LeverController));
            leverHitObj.transform.SetParent(machineRoot.transform, false);
            RectTransform leverHitRect = leverHitObj.GetComponent<RectTransform>();
            leverHitRect.anchorMin = new Vector2(0.5f, 0.5f);
            leverHitRect.anchorMax = new Vector2(0.5f, 0.5f);
            leverHitRect.pivot = new Vector2(0.5f, 0.5f);
            leverHitRect.anchoredPosition = new Vector2(310.5f, -118f);
            leverHitRect.sizeDelta = new Vector2(130f, 320f);
            Image leverHitImg = leverHitObj.GetComponent<Image>();
            leverHitImg.color = new Color(0f, 0f, 0f, 0f); // Invisible raycast target

            // ==========================================
            // LAYER 5: HUD PANEL / MIDDLE BOX (slot_machine_Middle_box.png)
            // ==========================================
            GameObject hudObj = new GameObject("HUDPanel", typeof(RectTransform), typeof(Image), typeof(MiddleBoxHUD));
            hudObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hudRect = hudObj.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 0.5f);
            hudRect.anchorMax = new Vector2(0.5f, 0.5f);
            hudRect.pivot = new Vector2(0.5f, 0.5f);
            hudRect.anchoredPosition = new Vector2(0f, -385f);
            hudRect.sizeDelta = new Vector2(658f, 240f);
            Image hudImg = hudObj.GetComponent<Image>();
            hudImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot_machine_Middle_box.png");

            // Text Labels inside HUD
            Text balText = CreateHUDLabel(hudObj.transform, "BalanceSection", "BALANCE", "₹1,00,000", new Vector2(-205f, 0f), new Color(1f, 0.85f, 0.3f));
            Text betText = CreateHUDLabel(hudObj.transform, "BetSection", "VIP BET", "₹500", new Vector2(0f, 0f), Color.white);
            Text winText = CreateHUDLabel(hudObj.transform, "WinSection", "LAST WIN", "₹0", new Vector2(205f, 0f), new Color(0.35f, 1f, 0.55f));

            // Controls: Left Arrow decreases bet, Right Arrow increases bet
            GameObject btnMinusObj = CreateButton(hudObj.transform, "Btn_BetMinus", new Vector2(-75f, -12f), new Vector2(48f, 48f), "Assets/slot_machine_buttons-04.png", "btn_bet_plus");
            GameObject btnPlusObj = CreateButton(hudObj.transform, "Btn_BetPlus", new Vector2(75f, -12f), new Vector2(48f, 48f), "Assets/slot_machine_buttons-03.png", "btn_bet_minus");

            // ==========================================
            // LAYER 6: HIGH-ROLLER GOLDEN SPIN BUTTON
            // ==========================================
            GameObject spinBtnObj = CreateGoldSpinButton(canvasObj.transform, new Vector2(460f, -385f), new Vector2(150f, 150f));

            // ==========================================
            // LAYER 7: PARTICLE SYSTEM & FX PRESENTER
            // ==========================================
            ParticleSystem goldParticles = CreateParticleSystem(canvasObj.transform);
            var fxComp = machineRoot.AddComponent<WinEffectsPresenter>();
            fxComp.Initialize(machineRect, goldParticles);

            // ==========================================
            // LAYER 8: PROCEDURAL AUDIO ENGINE
            // ==========================================
            var audioComp = machineRoot.AddComponent<AudioController>();

            // ==========================================
            // LAYER 9: MODAL POPUP LAYER (popup.png, Yes_No_Btn.png)
            // ==========================================
            WinPopupController popupController = CreateModalPopup(canvasObj.transform);

            // ==========================================
            // CORE GAME MACHINE CONTROLLER & WIRING
            // ==========================================
            var rngComp = machineRoot.AddComponent<RandomNumberGenerator>();
            var walletComp = machineRoot.AddComponent<WalletManager>();
            var controller = machineRoot.AddComponent<SlotMachineController>();
            controller.Configure(db, rngComp, walletComp, reelList, popupController, audioComp, fxComp);

            LeverController leverCtrl = leverHitObj.GetComponent<LeverController>();
            leverCtrl.Initialize(armRect, downImg, controller, audioComp);

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

            SpinButtonTrigger spinTrigger = spinBtnObj.GetComponent<SpinButtonTrigger>();
            if (spinTrigger != null)
            {
                spinTrigger.Initialize(controller);
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

        private static GameObject CreateGoldSpinButton(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = new GameObject("Btn_Spin", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            // Rich golden metallic circular button
            img.color = new Color(0.95f, 0.75f, 0.15f, 1f);

            // Inner dark emerald jewel
            GameObject innerObj = new GameObject("InnerJewel", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(btnObj.transform, false);
            RectTransform inRect = innerObj.GetComponent<RectTransform>();
            inRect.anchorMin = new Vector2(0.08f, 0.08f);
            inRect.anchorMax = new Vector2(0.92f, 0.92f);
            inRect.sizeDelta = Vector2.zero;
            Image inImg = innerObj.GetComponent<Image>();
            inImg.color = new Color(0.06f, 0.35f, 0.22f, 1f);
            inImg.raycastTarget = false;

            // SPIN Text
            GameObject txtObj = new GameObject("SpinText", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            Text txt = txtObj.GetComponent<Text>();
            txt.text = "SPIN\n₹";
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.95f, 0.6f, 1f);
            txt.raycastTarget = false;

            Button btn = btnObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            btn.colors = cb;

            btnObj.AddComponent<SpinButtonTrigger>();

            return btnObj;
        }

        private static ParticleSystem CreateParticleSystem(Transform parent)
        {
            GameObject psObj = new GameObject("CelebrationParticles", typeof(RectTransform), typeof(ParticleSystem));
            psObj.transform.SetParent(parent, false);
            RectTransform psRect = psObj.GetComponent<RectTransform>();
            psRect.anchoredPosition = new Vector2(0f, 75f);

            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.2f;
            main.startLifetime = 1.8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(250f, 650f);
            main.startSize = new ParticleSystem.MinMaxCurve(18f, 36f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.2f), new Color(1f, 0.65f, 0.1f));
            main.gravityModifier = 1.6f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 90f;

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
            bdImg.color = new Color(0f, 0f, 0f, 0.82f);

            // Modal Container
            GameObject container = new GameObject("ModalContainer", typeof(RectTransform), typeof(Image));
            container.transform.SetParent(modalRoot.transform, false);
            RectTransform cRect = container.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(740f, 440f);
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
            titleTxt.fontSize = 28;
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
            amountTxt.fontSize = 42;
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
            msgTxt.fontSize = 22;
            msgTxt.fontStyle = FontStyle.Normal;
            msgTxt.alignment = TextAnchor.MiddleCenter;
            msgTxt.color = Color.white;

            // Buttons (Yes & No)
            GameObject yesBtnObj = CreateButton(container.transform, "Btn_Yes", new Vector2(-110f, -115f), new Vector2(170f, 62f), "Assets/Yes_No_Btn.png", "btn_yes");
            GameObject noBtnObj = CreateButton(container.transform, "Btn_No", new Vector2(110f, -115f), new Vector2(170f, 62f), "Assets/Yes_No_Btn.png", "btn_no");

            WinPopupController ctrl = modalRoot.GetComponent<WinPopupController>();
            ctrl.Initialize(backdrop, cRect, titleTxt, msgTxt, amountTxt, yesBtnObj.GetComponent<Button>(), noBtnObj.GetComponent<Button>());
            backdrop.SetActive(false);
            modalRoot.SetActive(false);

            return ctrl;
        }

        private static GameObject CreateReelPrefab(SymbolDatabase db)
        {
            GameObject reelRoot = new GameObject("SlotReel", typeof(RectTransform), typeof(SlotReel));
            RectTransform rootRect = reelRoot.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(104f, 208f);

            GameObject symbolsContainer = new GameObject("SymbolsContainer", typeof(RectTransform));
            symbolsContainer.transform.SetParent(reelRoot.transform, false);
            RectTransform scRect = symbolsContainer.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.5f, 0.5f);
            scRect.anchorMax = new Vector2(0.5f, 0.5f);
            scRect.pivot = new Vector2(0.5f, 0.5f);
            scRect.anchoredPosition = Vector2.zero;
            scRect.sizeDelta = new Vector2(104f, 2000f);

            // Construct 20 ordered symbol slots on the strip (Y: 0, 100, 200, ... 1900)
            int totalSymbols = 20;
            var symbolList = new List<SlotSymbol>();

            for (int i = 0; i < totalSymbols; i++)
            {
                SymbolData symData = (db != null && db.Count > 0) ? db[i % db.Count] : null;

                GameObject slotObj = new GameObject($"SlotSymbol_{i}", typeof(RectTransform), typeof(SlotSymbol));
                slotObj.transform.SetParent(symbolsContainer.transform, false);
                RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.pivot = new Vector2(0.5f, 0.5f);
                slotRect.anchoredPosition = new Vector2(0f, i * 100f);
                slotRect.sizeDelta = new Vector2(98f, 96f);

                // Elegant subtle tile backing
                GameObject tileBgObj = new GameObject("TileBg", typeof(RectTransform), typeof(Image));
                tileBgObj.transform.SetParent(slotObj.transform, false);
                RectTransform tileRect = tileBgObj.GetComponent<RectTransform>();
                tileRect.anchorMin = Vector2.zero;
                tileRect.anchorMax = Vector2.one;
                tileRect.sizeDelta = Vector2.zero;
                Image tileImg = tileBgObj.GetComponent<Image>();
                tileImg.color = new Color(0.10f, 0.06f, 0.20f, 0.85f);
                tileImg.raycastTarget = false;

                // High-resolution Icon Image (78x78 perfectly centered inside 108px cutout)
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(slotObj.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(78f, 78f);
                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // Win Highlight Glow
                GameObject glowObj = new GameObject("Glow", typeof(RectTransform), typeof(Image));
                glowObj.transform.SetParent(slotObj.transform, false);
                RectTransform glowRect = glowObj.GetComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.sizeDelta = new Vector2(8f, 8f);
                Image glowImg = glowObj.GetComponent<Image>();
                glowImg.color = new Color(1f, 0.85f, 0.2f, 0.45f);
                glowImg.raycastTarget = false;
                glowObj.SetActive(false);

                SlotSymbol symComp = slotObj.GetComponent<SlotSymbol>();
                symComp.InitializeReferences(iconImg, glowImg, tileImg);
                if (symData != null)
                {
                    symComp.SetSymbol(symData);
                }

                symbolList.Add(symComp);
            }

            SlotReel reelComp = reelRoot.GetComponent<SlotReel>();
            reelComp.SetStripReferences(scRect, symbolList);

            string prefabPath = "Assets/Prefabs/ReelPrefab.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(reelRoot, prefabPath);
            GameObject.DestroyImmediate(reelRoot);
            Debug.Log($"[SpinRush Scene Setup] Created robust 20-symbol ReelPrefab at: {prefabPath}");
            return savedPrefab;
        }

        private static Text CreateHUDLabel(Transform parent, string name, string labelText, string defaultValue, Vector2 pos, Color valueColor)
        {
            GameObject secObj = new GameObject(name, typeof(RectTransform));
            secObj.transform.SetParent(parent, false);
            RectTransform secRect = secObj.GetComponent<RectTransform>();
            secRect.anchoredPosition = pos;
            secRect.sizeDelta = new Vector2(190f, 110f);

            // Title Label
            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblObj.transform.SetParent(secObj.transform, false);
            RectTransform lblRect = lblObj.GetComponent<RectTransform>();
            lblRect.anchoredPosition = new Vector2(0f, 25f);
            lblRect.sizeDelta = new Vector2(180f, 30f);
            Text lblText = lblObj.GetComponent<Text>();
            lblText.text = labelText;
            lblText.fontSize = 20;
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
