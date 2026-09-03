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
    /// arcade shortcuts panel, interactive spotlight onboarding tutorial, procedural audio, and win celebration effects.
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
            machineRect.anchoredPosition = new Vector2(45f, 75f);
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
            hudRect.anchoredPosition = new Vector2(45f, -385f);
            hudRect.sizeDelta = new Vector2(760f, 240f); // Extended width to prevent border overlap
            Image hudImg = hudObj.GetComponent<Image>();
            hudImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/slot_machine_Middle_box.png");

            // Text Labels inside HUD with spacious margins
            Text balText = CreateHUDLabel(hudObj.transform, "BalanceSection", "BALANCE", "₹1,00,000", new Vector2(-230f, 0f), new Color(1f, 0.85f, 0.3f));
            Text betText = CreateHUDLabel(hudObj.transform, "BetSection", "VIP BET", "₹500", new Vector2(0f, 0f), Color.white);
            Text winText = CreateHUDLabel(hudObj.transform, "WinSection", "LAST WIN", "₹0", new Vector2(230f, 0f), new Color(0.35f, 1f, 0.55f));

            // Controls: Left Arrow decreases bet, Right Arrow increases bet
            GameObject btnMinusObj = CreateButton(hudObj.transform, "Btn_BetMinus", new Vector2(-75f, -12f), new Vector2(48f, 48f), "Assets/slot_machine_buttons-04.png", "btn_bet_plus");
            GameObject btnPlusObj = CreateButton(hudObj.transform, "Btn_BetPlus", new Vector2(75f, -12f), new Vector2(48f, 48f), "Assets/slot_machine_buttons-03.png", "btn_bet_minus");

            // Dedicated Spotlight Target for Jackpot Marquee
            GameObject jackpotTarget = new GameObject("JackpotTarget", typeof(RectTransform));
            jackpotTarget.transform.SetParent(machineRoot.transform, false);
            RectTransform jpRect = jackpotTarget.GetComponent<RectTransform>();
            jpRect.anchoredPosition = new Vector2(45f, 215f);
            jpRect.sizeDelta = new Vector2(430f, 125f);

            // ==========================================
            // LAYER 6: PARTICLE SYSTEM & FX PRESENTER
            // ==========================================
            ParticleSystem goldParticles = CreateParticleSystem(canvasObj.transform);
            var fxComp = machineRoot.AddComponent<WinEffectsPresenter>();
            fxComp.Initialize(machineRect, goldParticles);

            // ==========================================
            // LAYER 7: PROCEDURAL AUDIO ENGINE
            // ==========================================
            var audioComp = machineRoot.AddComponent<AudioController>();

            // ==========================================
            // LAYER 8: MODAL WIN CELEBRATION LAYER
            // ==========================================
            WinPopupController popupController = CreateModalPopup(canvasObj.transform);

            // ==========================================
            // LAYER 9: ONBOARDING TUTORIAL LAYER WITH SPOTLIGHT
            // ==========================================
            TutorialManager tutorialController = CreateTutorialLayer(canvasObj.transform, vpRect, hudRect, leverHitRect, jpRect, audioComp);

            // ==========================================
            // LAYER 10: ARCADE SHORTCUTS PANEL (DOCKED ON LEFT)
            // ==========================================
            CreateShortcutsPanel(canvasObj.transform, tutorialController);

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

        private static GameObject CreateShortcutsPanel(Transform parent, TutorialManager tutorialMgr)
        {
            // Root panel docked on left
            GameObject panelObj = new GameObject("ShortcutsPanel", typeof(RectTransform), typeof(Image), typeof(ShortcutsPanel));
            panelObj.transform.SetParent(parent, false);
            RectTransform pRect = panelObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0f, 0.5f);
            pRect.anchorMax = new Vector2(0f, 0.5f);
            pRect.pivot = new Vector2(0f, 0.5f);
            pRect.anchoredPosition = new Vector2(30f, 0f);
            pRect.sizeDelta = new Vector2(260f, 480f);

            Image pImg = panelObj.GetComponent<Image>();
            pImg.color = new Color(0.05f, 0.03f, 0.14f, 0.95f); // Rich dark arcade chassis

            // Outer multi-layer glowing gold & cyan neon border
            Outline outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            panelShadow.effectDistance = new Vector2(4f, -4f);

            // Header Banner Plaque
            GameObject headerPlate = new GameObject("HeaderPlate", typeof(RectTransform), typeof(Image), typeof(Outline));
            headerPlate.transform.SetParent(panelObj.transform, false);
            RectTransform hpRect = headerPlate.GetComponent<RectTransform>();
            hpRect.anchoredPosition = new Vector2(0f, 205f);
            hpRect.sizeDelta = new Vector2(236f, 44f);
            Image hpImg = headerPlate.GetComponent<Image>();
            hpImg.color = new Color(0.14f, 0.08f, 0.32f, 1f); // Metallic violet plaque
            Outline hpOutline = headerPlate.GetComponent<Outline>();
            hpOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.7f);
            hpOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Header Title Text
            GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(Text), typeof(Shadow));
            headerObj.transform.SetParent(headerPlate.transform, false);
            RectTransform hRect = headerObj.GetComponent<RectTransform>();
            hRect.anchorMin = Vector2.zero;
            hRect.anchorMax = Vector2.one;
            hRect.sizeDelta = Vector2.zero;
            Text hTxt = headerObj.GetComponent<Text>();
            hTxt.text = "ARCADE CONTROLS";
            hTxt.fontSize = 17;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.35f);
            Shadow hShadow = headerObj.GetComponent<Shadow>();
            hShadow.effectColor = new Color(0f, 0f, 0.05f, 0.9f);
            hShadow.effectDistance = new Vector2(1f, -1f);

            // Shortcuts items
            float startY = 150f;
            float rowHeight = 56f;
            string[,] shortcuts = new string[,]
            {
                { "SPACE / ENTER", "Pull Lever" },
                { "DRAG MOUSE", "Manual Pull" },
                { "LEFT / DOWN", "VIP Bet -" },
                { "RIGHT / UP", "VIP Bet +" },
                { "H", "Tutorial" }
            };

            for (int i = 0; i < shortcuts.GetLength(0); i++)
            {
                float y = startY - (i * rowHeight);

                // Row container
                GameObject rowObj = new GameObject($"Row_{i}", typeof(RectTransform), typeof(Image));
                rowObj.transform.SetParent(panelObj.transform, false);
                RectTransform rRect = rowObj.GetComponent<RectTransform>();
                rRect.anchoredPosition = new Vector2(0f, y);
                rRect.sizeDelta = new Vector2(236f, 48f);
                Image rImg = rowObj.GetComponent<Image>();
                rImg.color = (i % 2 == 0) ? new Color(0.09f, 0.06f, 0.22f, 0.5f) : new Color(0.06f, 0.04f, 0.16f, 0.3f);

                // Keycap badge with 3D bevel and cyan neon border
                GameObject keyObj = new GameObject("KeyBadge", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
                keyObj.transform.SetParent(rowObj.transform, false);
                RectTransform kRect = keyObj.GetComponent<RectTransform>();
                kRect.anchorMin = new Vector2(0f, 0.5f);
                kRect.anchorMax = new Vector2(0f, 0.5f);
                kRect.pivot = new Vector2(0f, 0.5f);
                kRect.anchoredPosition = new Vector2(8f, 0f);
                kRect.sizeDelta = new Vector2(100f, 34f);
                Image kImg = keyObj.GetComponent<Image>();
                kImg.color = new Color(0.18f, 0.14f, 0.38f, 1f); // 3D arcade keycap base
                Outline kOutline = keyObj.GetComponent<Outline>();
                kOutline.effectColor = new Color(0f, 0.85f, 1f, 0.75f); // Neon cyan outline
                kOutline.effectDistance = new Vector2(1f, -1f);
                Shadow kShadow = keyObj.GetComponent<Shadow>();
                kShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
                kShadow.effectDistance = new Vector2(1f, -2f);

                GameObject kTxtObj = new GameObject("KeyText", typeof(RectTransform), typeof(Text));
                kTxtObj.transform.SetParent(keyObj.transform, false);
                RectTransform ktRect = kTxtObj.GetComponent<RectTransform>();
                ktRect.anchorMin = Vector2.zero;
                ktRect.anchorMax = Vector2.one;
                ktRect.sizeDelta = Vector2.zero;
                Text kt = kTxtObj.GetComponent<Text>();
                kt.text = shortcuts[i, 0];
                kt.fontSize = 13;
                kt.fontStyle = FontStyle.Bold;
                kt.alignment = TextAnchor.MiddleCenter;
                kt.color = Color.white;

                // Description text
                GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(Text), typeof(Shadow));
                descObj.transform.SetParent(rowObj.transform, false);
                RectTransform dRect = descObj.GetComponent<RectTransform>();
                dRect.anchorMin = new Vector2(0f, 0.5f);
                dRect.anchorMax = new Vector2(1f, 0.5f);
                dRect.pivot = new Vector2(0f, 0.5f);
                dRect.anchoredPosition = new Vector2(118f, 0f);
                dRect.sizeDelta = new Vector2(-122f, 34f);
                Text dt = descObj.GetComponent<Text>();
                dt.text = shortcuts[i, 1];
                dt.fontSize = 14;
                dt.fontStyle = FontStyle.Bold;
                dt.alignment = TextAnchor.MiddleLeft;
                dt.color = new Color(0.96f, 0.90f, 0.65f); // Champagne gold
                Shadow dShadow = descObj.GetComponent<Shadow>();
                dShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                dShadow.effectDistance = new Vector2(1f, -1f);

                // Row Divider Hairline
                GameObject divObj = new GameObject("Divider", typeof(RectTransform), typeof(Image));
                divObj.transform.SetParent(rowObj.transform, false);
                RectTransform divRect = divObj.GetComponent<RectTransform>();
                divRect.anchorMin = new Vector2(0.05f, 0f);
                divRect.anchorMax = new Vector2(0.95f, 0f);
                divRect.pivot = new Vector2(0.5f, 0f);
                divRect.sizeDelta = new Vector2(0f, 1f);
                Image divImg = divObj.GetComponent<Image>();
                divImg.color = new Color(0.35f, 0.25f, 0.60f, 0.35f);
            }

            // Interactive Help / How to play arcade button
            GameObject helpBtnObj = new GameObject("Btn_Help", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            helpBtnObj.transform.SetParent(panelObj.transform, false);
            RectTransform hBtnRect = helpBtnObj.GetComponent<RectTransform>();
            hBtnRect.anchoredPosition = new Vector2(0f, -200f);
            hBtnRect.sizeDelta = new Vector2(220f, 48f);
            Image hBtnImg = helpBtnObj.GetComponent<Image>();
            hBtnImg.color = new Color(0.08f, 0.45f, 0.28f, 1f); // Vibrant emerald push button
            Outline hBtnOutline = helpBtnObj.GetComponent<Outline>();
            hBtnOutline.effectColor = new Color(0.35f, 1f, 0.65f, 0.85f);
            hBtnOutline.effectDistance = new Vector2(2f, -2f);
            Shadow hBtnShadow = helpBtnObj.GetComponent<Shadow>();
            hBtnShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            hBtnShadow.effectDistance = new Vector2(2f, -3f);

            GameObject hBtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
            hBtnTxtObj.transform.SetParent(helpBtnObj.transform, false);
            RectTransform hbtRect = hBtnTxtObj.GetComponent<RectTransform>();
            hbtRect.anchorMin = Vector2.zero;
            hbtRect.anchorMax = Vector2.one;
            hbtRect.sizeDelta = Vector2.zero;
            Text hbt = hBtnTxtObj.GetComponent<Text>();
            hbt.text = "HOW TO PLAY";
            hbt.fontSize = 16;
            hbt.fontStyle = FontStyle.Bold;
            hbt.alignment = TextAnchor.MiddleCenter;
            hbt.color = new Color(1f, 0.98f, 0.80f);
            Shadow hbtShadow = hBtnTxtObj.GetComponent<Shadow>();
            hbtShadow.effectColor = new Color(0f, 0.2f, 0.1f, 0.9f);
            hbtShadow.effectDistance = new Vector2(1f, -1f);

            Button hBtn = helpBtnObj.GetComponent<Button>();
            if (tutorialMgr != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(hBtn.onClick, tutorialMgr.StartTutorial);
            }

            ShortcutsPanel shortcutsComp = panelObj.GetComponent<ShortcutsPanel>();
            shortcutsComp.Initialize(tutorialMgr);

            return panelObj;
        }

        private static TutorialManager CreateTutorialLayer(
            Transform parent,
            RectTransform reelsTarget,
            RectTransform hudTarget,
            RectTransform leverTarget,
            RectTransform cabinetTarget,
            AudioController audio)
        {
            // Root
            GameObject tutRoot = new GameObject("TutorialLayer", typeof(RectTransform), typeof(TutorialManager));
            tutRoot.transform.SetParent(parent, false);
            RectTransform tutRect = tutRoot.GetComponent<RectTransform>();
            tutRect.anchorMin = Vector2.zero;
            tutRect.anchorMax = Vector2.one;
            tutRect.sizeDelta = Vector2.zero;

            // Dim Backdrop
            GameObject dimObj = new GameObject("DimBackdrop", typeof(RectTransform), typeof(Image));
            dimObj.transform.SetParent(tutRoot.transform, false);
            RectTransform dimRect = dimObj.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            Image dimImg = dimObj.GetComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0            // Spotlight Box (Dynamic outline highlighting target with transparent interior)
            GameObject spotObj = new GameObject("SpotlightBox", typeof(RectTransform), typeof(Image), typeof(Outline));
            spotObj.transform.SetParent(tutRoot.transform, false);
            RectTransform spotRect = spotObj.GetComponent<RectTransform>();
            spotRect.sizeDelta = new Vector2(380f, 220f);
            spotRect.anchoredPosition = Vector2.zero;
            Image spotImg = spotObj.GetComponent<Image>();
            spotImg.color = new Color(1f, 0.85f, 0.2f, 0.01f); // Transparent interior
            spotImg.raycastTarget = false;

            Outline spotOutline = spotObj.GetComponent<Outline>();
            spotOutline.effectColor = new Color(1f, 0.85f, 0.25f, 0.95f);
            spotOutline.effectDistance = new Vector2(3.5f, -3.5f);

            // Dialogue Container Card
            GameObject cardObj = new GameObject("DialogueCard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            cardObj.transform.SetParent(tutRoot.transform, false);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(620f, 270f);
            cardRect.anchoredPosition = new Vector2(45f, -170f);
            Image cardImg = cardObj.GetComponent<Image>();
            cardImg.color = new Color(0.07f, 0.04f, 0.18f, 0.97f); // Deep arcade obsidian glass

            // Double Beveled Neon Border
            Outline cOutline = cardObj.GetComponent<Outline>();
            cOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.9f); // Gold neon
            cOutline.effectDistance = new Vector2(2.5f, -2.5f);

            Shadow cShadow = cardObj.GetComponent<Shadow>();
            cShadow.effectColor = new Color(0f, 0f, 0.05f, 0.8f);
            cShadow.effectDistance = new Vector2(4f, -4f);

            // Header Plaque Badge
            GameObject headPlaque = new GameObject("HeaderPlaque", typeof(RectTransform), typeof(Image), typeof(Outline));
            headPlaque.transform.SetParent(cardObj.transform, false);
            RectTransform hpRect = headPlaque.GetComponent<RectTransform>();
            hpRect.anchoredPosition = new Vector2(0f, 135f);
            hpRect.sizeDelta = new Vector2(260f, 32f);
            Image hpImg = headPlaque.GetComponent<Image>();
            hpImg.color = new Color(0.16f, 0.10f, 0.38f, 1f);
            Outline hpOutline = headPlaque.GetComponent<Outline>();
            hpOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.8f);
            hpOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject hpTxtObj = new GameObject("PlaqueText", typeof(RectTransform), typeof(Text));
            hpTxtObj.transform.SetParent(headPlaque.transform, false);
            RectTransform hptRect = hpTxtObj.GetComponent<RectTransform>();
            hptRect.anchorMin = Vector2.zero;
            hptRect.anchorMax = Vector2.one;
            hptRect.sizeDelta = Vector2.zero;
            Text hpt = hpTxtObj.GetComponent<Text>();
            hpt.text = "ARCADE GUIDE";
            hpt.fontSize = 14;
            hpt.fontStyle = FontStyle.Bold;
            hpt.alignment = TextAnchor.MiddleCenter;
            hpt.color = new Color(1f, 0.90f, 0.40f);

            // Step Title Text
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Shadow));
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(0f, 85f);
            tRect.sizeDelta = new Vector2(580f, 40f);
            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.88f, 0.35f);
            Shadow tShadow = titleObj.GetComponent<Shadow>();
            tShadow.effectColor = new Color(0f, 0f, 0.05f, 0.9f);
            tShadow.effectDistance = new Vector2(1f, -1f);

            // Description Text
            GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(Text));
            descObj.transform.SetParent(cardObj.transform, false);
            RectTransform dRect = descObj.GetComponent<RectTransform>();
            dRect.anchoredPosition = new Vector2(0f, 25f);
            dRect.sizeDelta = new Vector2(560f, 65f);
            Text descTxt = descObj.GetComponent<Text>();
            descTxt.fontSize = 18;
            descTxt.fontStyle = FontStyle.Normal;
            descTxt.alignment = TextAnchor.MiddleCenter;
            descTxt.color = new Color(0.92f, 0.90f, 0.98f);

            // Step Dots Text (● ○ ○ ○)
            GameObject dotsObj = new GameObject("StepDotsText", typeof(RectTransform), typeof(Text));
            dotsObj.transform.SetParent(cardObj.transform, false);
            RectTransform dotsRect = dotsObj.GetComponent<RectTransform>();
            dotsRect.anchoredPosition = new Vector2(0f, -25f);
            dotsRect.sizeDelta = new Vector2(220f, 30f);
            Text dotsTxt = dotsObj.GetComponent<Text>();
            dotsTxt.fontSize = 20;
            dotsTxt.fontStyle = FontStyle.Bold;
            dotsTxt.alignment = TextAnchor.MiddleCenter;
            dotsTxt.color = Color.white;

            // Skip Button (Ghost arcade button)
            GameObject skipBtnObj = new GameObject("Btn_Skip", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            skipBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform sBtnRect = skipBtnObj.GetComponent<RectTransform>();
            sBtnRect.anchoredPosition = new Vector2(-180f, -80f);
            sBtnRect.sizeDelta = new Vector2(130f, 44f);
            Image sBtnImg = skipBtnObj.GetComponent<Image>();
            sBtnImg.color = new Color(0.20f, 0.14f, 0.36f, 0.95f);
            Outline sBtnOutline = skipBtnObj.GetComponent<Outline>();
            sBtnOutline.effectColor = new Color(0.5f, 0.4f, 0.75f, 0.7f);
            sBtnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject sBtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            sBtnTxtObj.transform.SetParent(skipBtnObj.transform, false);
            RectTransform sbtRect = sBtnTxtObj.GetComponent<RectTransform>();
            sbtRect.anchorMin = Vector2.zero;
            sbtRect.anchorMax = Vector2.one;
            sbtRect.sizeDelta = Vector2.zero;
            Text sbt = sBtnTxtObj.GetComponent<Text>();
            sbt.text = "SKIP";
            sbt.fontSize = 16;
            sbt.fontStyle = FontStyle.Bold;
            sbt.alignment = TextAnchor.MiddleCenter;
            sbt.color = new Color(0.85f, 0.85f, 0.95f);

            // Next Button (Vibrant arcade gold push button)
            GameObject nextBtnObj = new GameObject("Btn_Next", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            nextBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform nBtnRect = nextBtnObj.GetComponent<RectTransform>();
            nBtnRect.anchoredPosition = new Vector2(180f, -80f);
            nBtnRect.sizeDelta = new Vector2(170f, 44f);
            Image nBtnImg = nextBtnObj.GetComponent<Image>();
            nBtnImg.color = new Color(0.95f, 0.72f, 0.15f, 1f); // Radiant Gold
            Outline nBtnOutline = nextBtnObj.GetComponent<Outline>();
            nBtnOutline.effectColor = new Color(1f, 0.90f, 0.45f, 0.9f);
            nBtnOutline.effectDistance = new Vector2(1.5f, -1.5f);
            Shadow nBtnShadow = nextBtnObj.GetComponent<Shadow>();
            nBtnShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            nBtnShadow.effectDistance = new Vector2(2f, -2f);

            GameObject nBtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
            nBtnTxtObj.transform.SetParent(nextBtnObj.transform, false);
            RectTransform nbtRect = nBtnTxtObj.GetComponent<RectTransform>();
            nbtRect.anchorMin = Vector2.zero;
            nbtRect.anchorMax = Vector2.one;
            nbtRect.sizeDelta = Vector2.zero;
            Text nbt = nBtnTxtObj.GetComponent<Text>();
            nbt.text = "NEXT >";
            nbt.fontSize = 17;
            nbt.fontStyle = FontStyle.Bold;
            nbt.alignment = TextAnchor.MiddleCenter;
            nbt.color = new Color(0.12f, 0.08f, 0.22f);
            Shadow nbtShadow = nBtnTxtObj.GetComponent<Shadow>();
            nbtShadow.effectColor = new Color(1f, 0.9f, 0.5f, 0.5f);
            nbtShadow.effectDistance = new Vector2(1f, -1f);

            // "Do not show again" Toggle
            GameObject toggleObj = new GameObject("Toggle_DoNotShow", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(cardObj.transform, false);
            RectTransform togRect = toggleObj.GetComponent<RectTransform>();
            togRect.anchoredPosition = new Vector2(0f, -80f);
            togRect.sizeDelta = new Vector2(160f, 32f);

            GameObject boxObj = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(Outline));
            boxObj.transform.SetParent(toggleObj.transform, false);
            RectTransform boxRect = boxObj.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(0f, 0f);
            boxRect.sizeDelta = new Vector2(22f, 22f);
            Image boxImg = boxObj.GetComponent<Image>();
            boxImg.color = new Color(0.18f, 0.12f, 0.35f, 1f);
            Outline boxOutline = boxObj.GetComponent<Outline>();
            boxOutline.effectColor = new Color(0.4f, 0.3f, 0.65f, 0.8f);
            boxOutline.effectDistance = new Vector2(1f, -1f);

            GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkObj.transform.SetParent(boxObj.transform, false);
            RectTransform checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkRect.sizeDelta = Vector2.zero;
            Image checkImg = checkObj.GetComponent<Image>();
            checkImg.color = new Color(0.35f, 1f, 0.55f); // Glowing emerald checkmark

            GameObject togTxtObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            togTxtObj.transform.SetParent(toggleObj.transform, false);
            RectTransform ttRect = togTxtObj.GetComponent<RectTransform>();
            ttRect.anchorMin = new Vector2(0f, 0.5f);
            ttRect.anchorMax = new Vector2(1f, 0.5f);
            ttRect.pivot = new Vector2(0f, 0.5f);
            ttRect.anchoredPosition = new Vector2(28f, 0f);
            ttRect.sizeDelta = new Vector2(-28f, 26f);
            Text togTxt = togTxtObj.GetComponent<Text>();
            togTxt.text = "Don't show again";
            togTxt.fontSize = 14;
            togTxt.fontStyle = FontStyle.Normal;
            togTxt.alignment = TextAnchor.MiddleLeft;
            togTxt.color = new Color(0.88f, 0.88f, 0.94f);

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.graphic = checkImg;
            toggle.targetGraphic = boxImg;

            // Build 4 Interactive Tutorial Steps (No currency explanations, precise targets)
            var steps = new TutorialManager.TutorialStep[]
            {
                new TutorialManager.TutorialStep
                {
                    stepTitle = "STEP 1: THE REELS & PAYLINE",
                    stepDescription = "Match 3 identical symbols along the central horizontal payline to score massive rewards!",
                    targetElement = reelsTarget,
                    dialogueOffset = new Vector2(0f, -220f)
                },
                new TutorialManager.TutorialStep
                {
                    stepTitle = "STEP 2: VIP BET & BALANCE",
                    stepDescription = "Use the Left/Right arrows (or keyboard [LEFT] [RIGHT]) to adjust your VIP Bet from 100 up to 5,000 credits!",
                    targetElement = hudTarget,
                    dialogueOffset = new Vector2(0f, 210f)
                },
                new TutorialManager.TutorialStep
                {
                    stepTitle = "STEP 3: THE MECHANICAL LEVER",
                    stepDescription = "Click or pull the mechanical lever downward (or press [SPACEBAR]) to launch the spin!",
                    targetElement = leverTarget,
                    dialogueOffset = new Vector2(-200f, 0f)
                },
                new TutorialManager.TutorialStep
                {
                    stepTitle = "STEP 4: KOHINOOR JACKPOT",
                    stepDescription = "Line up 3 Kohinoor Diamonds for the colossal 100x Royal Dhamaka Jackpot!",
                    targetElement = cabinetTarget,
                    dialogueOffset = new Vector2(0f, -240f)
                }
            };

            TutorialManager tutMgr = tutRoot.GetComponent<TutorialManager>();
            tutMgr.Initialize(
                tutRoot,
                spotRect,
                spotImg,
                cardRect,
                titleTxt,
                descTxt,
                dotsTxt,
                nextBtnObj.GetComponent<Button>(),
                nbt,
                skipBtnObj.GetComponent<Button>(),
                toggle,
                steps,
                audio);

            return tutMgr;
        }

        private static ParticleSystem CreateParticleSystem(Transform parent)
        {
            GameObject psObj = new GameObject("CelebrationParticles", typeof(RectTransform), typeof(ParticleSystem));
            psObj.transform.SetParent(parent, false);
            RectTransform psRect = psObj.GetComponent<RectTransform>();
            psRect.anchoredPosition = new Vector2(45f, 75f);

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
            GameObject yTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            yTxtObj.transform.SetParent(yesBtnObj.transform, false);
            RectTransform ytRect = yTxtObj.GetComponent<RectTransform>();
            ytRect.anchorMin = Vector2.zero;
            ytRect.anchorMax = Vector2.one;
            ytRect.sizeDelta = Vector2.zero;
            Text yt = yTxtObj.GetComponent<Text>();
            yt.text = "COLLECT";
            yt.fontSize = 20;
            yt.fontStyle = FontStyle.Bold;
            yt.alignment = TextAnchor.MiddleCenter;
            yt.color = new Color(0.12f, 0.08f, 0.22f);

            GameObject noBtnObj = CreateButton(container.transform, "Btn_No", new Vector2(110f, -115f), new Vector2(170f, 62f), "Assets/Yes_No_Btn.png", "btn_no");
            GameObject nTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            nTxtObj.transform.SetParent(noBtnObj.transform, false);
            RectTransform ntRect = nTxtObj.GetComponent<RectTransform>();
            ntRect.anchorMin = Vector2.zero;
            ntRect.anchorMax = Vector2.one;
            ntRect.sizeDelta = Vector2.zero;
            Text nt = nTxtObj.GetComponent<Text>();
            nt.text = "CANCEL";
            nt.fontSize = 20;
            nt.fontStyle = FontStyle.Bold;
            nt.alignment = TextAnchor.MiddleCenter;
            nt.color = new Color(0.12f, 0.08f, 0.22f);

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
            secRect.sizeDelta = new Vector2(230f, 110f); // Generous width

            // Title Label
            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblObj.transform.SetParent(secObj.transform, false);
            RectTransform lblRect = lblObj.GetComponent<RectTransform>();
            lblRect.anchoredPosition = new Vector2(0f, 25f);
            lblRect.sizeDelta = new Vector2(220f, 30f);
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
            valRect.sizeDelta = new Vector2(220f, 40f);
            Text valText = valObj.GetComponent<Text>();
            valText.text = defaultValue;
            valText.fontSize = 26;
            valText.fontStyle = FontStyle.Bold;
            valText.alignment = TextAnchor.MiddleCenter;
            valText.color = valueColor;
            valText.resizeTextForBestFit = true;
            valText.resizeTextMinSize = 16;
            valText.resizeTextMaxSize = 26;

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
