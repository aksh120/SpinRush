using UnityEngine;
using UnityEngine.UI;

namespace SpinRush.UI
{
    /// <summary>
    /// Displays an arcade-styled keyboard controls and shortcuts guide on the left side of the screen.
    /// Also provides a quick-access button to trigger the tutorial.
    /// </summary>
    public class ShortcutsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private Button helpButton;
        [SerializeField] private RectTransform panelContainer;

        private void Awake()
        {
            if (tutorialManager == null)
            {
                tutorialManager = FindObjectOfType<TutorialManager>();
            }

            BuildLuxuryArcadePanel();
        }

        /// <summary>
        /// Procedurally constructs a state-of-the-art luxury casino arcade controls panel.
        /// Features dark obsidian glass, 3D neon cyan keycaps, champagne gold typography, and an emerald arcade push button.
        /// </summary>
        private void BuildLuxuryArcadePanel()
        {
            // Configure root RectTransform
            RectTransform panelRect = GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = new Vector2(216f, 380f);
            }

            // Obsidian Glass Chassis
            Image panelImg = GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.04f, 0.02f, 0.12f, 0.96f);
            }

            Outline panelOutline = GetComponent<Outline>();
            if (panelOutline == null) panelOutline = gameObject.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.96f, 0.78f, 0.22f, 0.95f); // Radiant Gold
            panelOutline.effectDistance = new Vector2(2.5f, -2.5f);

            Shadow panelShadow = GetComponent<Shadow>();
            if (panelShadow == null) panelShadow = gameObject.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0.02f, 0.08f, 0.85f);
            panelShadow.effectDistance = new Vector2(5f, -5f);

            // Clean up any old children to guarantee pristine arcade layout
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Header Plaque
            GameObject plaqueObj = new GameObject("HeaderPlaque", typeof(RectTransform), typeof(Image), typeof(Outline));
            plaqueObj.transform.SetParent(transform, false);
            RectTransform pRect = plaqueObj.GetComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(0f, 158f);
            pRect.sizeDelta = new Vector2(200f, 34f);
            Image pImg = plaqueObj.GetComponent<Image>();
            pImg.color = new Color(0.12f, 0.08f, 0.28f, 1f);
            Outline pOutline = plaqueObj.GetComponent<Outline>();
            pOutline.effectColor = new Color(1f, 0.85f, 0.2f, 0.8f);
            pOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject pTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
            pTxtObj.transform.SetParent(plaqueObj.transform, false);
            RectTransform ptRect = pTxtObj.GetComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            ptRect.sizeDelta = Vector2.zero;
            Text ptText = pTxtObj.GetComponent<Text>();
            ptText.font = font;
            ptText.text = "ARCADE CONTROLS";
            ptText.fontSize = 13;
            ptText.fontStyle = FontStyle.Bold;
            ptText.alignment = TextAnchor.MiddleCenter;
            ptText.color = new Color(1f, 0.92f, 0.45f);
            Shadow ptShadow = pTxtObj.GetComponent<Shadow>();
            ptShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            ptShadow.effectDistance = new Vector2(1f, -1f);

            // 2. Gold Divider Hairline
            GameObject divObj = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divObj.transform.SetParent(transform, false);
            RectTransform divRect = divObj.GetComponent<RectTransform>();
            divRect.anchoredPosition = new Vector2(0f, 134f);
            divRect.sizeDelta = new Vector2(196f, 2f);
            Image divImg = divObj.GetComponent<Image>();
            divImg.color = new Color(0.95f, 0.75f, 0.2f, 0.55f);

            // 3. Four Illuminated Control Rows
            string[,] controls = new string[,]
            {
                { "SPACE / ENTER", "Pull Lever" },
                { "DRAG MOUSE", "Manual Pull" },
                { "[ < ]   [ > ]", "VIP Bet +/-" },
                { "[ H ] / [ F1 ]", "Game Guide" }
            };

            for (int r = 0; r < 4; r++)
            {
                float rowY = 98f - r * 46f;

                // Row Plate
                GameObject rowObj = new GameObject($"Row_{r}", typeof(RectTransform), typeof(Image));
                rowObj.transform.SetParent(transform, false);
                RectTransform rRect = rowObj.GetComponent<RectTransform>();
                rRect.anchoredPosition = new Vector2(0f, rowY);
                rRect.sizeDelta = new Vector2(200f, 38f);
                Image rImg = rowObj.GetComponent<Image>();
                rImg.color = (r % 2 == 0) ? new Color(0.12f, 0.08f, 0.26f, 0.65f) : new Color(0.08f, 0.05f, 0.18f, 0.65f);

                // 3D Keycap Badge
                GameObject keyObj = new GameObject("Keycap", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
                keyObj.transform.SetParent(rowObj.transform, false);
                RectTransform kRect = keyObj.GetComponent<RectTransform>();
                kRect.anchoredPosition = new Vector2(-48f, 0f);
                kRect.sizeDelta = new Vector2(92f, 26f);
                Image kImg = keyObj.GetComponent<Image>();
                kImg.color = new Color(0.16f, 0.12f, 0.32f, 1f);
                Outline kOutline = keyObj.GetComponent<Outline>();
                kOutline.effectColor = new Color(0f, 0.90f, 1f, 0.85f); // Neon Cyan
                kOutline.effectDistance = new Vector2(1.2f, -1.2f);
                Shadow kShadow = keyObj.GetComponent<Shadow>();
                kShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
                kShadow.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject ktObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                ktObj.transform.SetParent(keyObj.transform, false);
                RectTransform ktRect = ktObj.GetComponent<RectTransform>();
                ktRect.anchorMin = Vector2.zero;
                ktRect.anchorMax = Vector2.one;
                ktRect.sizeDelta = Vector2.zero;
                Text kt = ktObj.GetComponent<Text>();
                kt.font = font;
                kt.text = controls[r, 0];
                kt.fontSize = 10;
                kt.fontStyle = FontStyle.Bold;
                kt.alignment = TextAnchor.MiddleCenter;
                kt.color = Color.white;

                // Action Label
                GameObject actObj = new GameObject("ActionLabel", typeof(RectTransform), typeof(Text), typeof(Shadow));
                actObj.transform.SetParent(rowObj.transform, false);
                RectTransform actRect = actObj.GetComponent<RectTransform>();
                actRect.anchoredPosition = new Vector2(52f, 0f);
                actRect.sizeDelta = new Vector2(90f, 26f);
                Text actText = actObj.GetComponent<Text>();
                actText.font = font;
                actText.text = controls[r, 1];
                actText.fontSize = 12;
                actText.fontStyle = FontStyle.Bold;
                actText.alignment = TextAnchor.MiddleLeft;
                actText.color = new Color(0.96f, 0.90f, 0.65f); // Radiant Champagne Gold
                Shadow actShadow = actObj.GetComponent<Shadow>();
                actShadow.effectColor = new Color(0f, 0f, 0.05f, 0.9f);
                actShadow.effectDistance = new Vector2(1f, -1f);
            }

            // 4. Emerald Arcade Push Button
            GameObject helpBtnObj = new GameObject("Btn_Help", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            helpBtnObj.transform.SetParent(transform, false);
            RectTransform hRect = helpBtnObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, -118f);
            hRect.sizeDelta = new Vector2(186f, 44f);
            Image hImg = helpBtnObj.GetComponent<Image>();
            hImg.color = new Color(0f, 0.68f, 0.42f, 1f); // Vibrant Emerald
            Outline hOutline = helpBtnObj.GetComponent<Outline>();
            hOutline.effectColor = new Color(0.4f, 1f, 0.65f, 0.95f);
            hOutline.effectDistance = new Vector2(2f, -2f);
            Shadow hShadow = helpBtnObj.GetComponent<Shadow>();
            hShadow.effectColor = new Color(0f, 0.2f, 0.1f, 0.9f);
            hShadow.effectDistance = new Vector2(3f, -3f);

            GameObject hTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
            hTxtObj.transform.SetParent(helpBtnObj.transform, false);
            RectTransform htRect = hTxtObj.GetComponent<RectTransform>();
            htRect.anchorMin = Vector2.zero;
            htRect.anchorMax = Vector2.one;
            htRect.sizeDelta = Vector2.zero;
            Text htText = hTxtObj.GetComponent<Text>();
            htText.font = font;
            htText.text = "HOW TO PLAY";
            htText.fontSize = 15;
            htText.fontStyle = FontStyle.Bold;
            htText.alignment = TextAnchor.MiddleCenter;
            htText.color = new Color(1f, 0.98f, 0.85f);
            Shadow htShadow = hTxtObj.GetComponent<Shadow>();
            htShadow.effectColor = new Color(0f, 0.2f, 0.1f, 0.9f);
            htShadow.effectDistance = new Vector2(1f, -1f);

            helpButton = helpBtnObj.GetComponent<Button>();
            helpButton.onClick.AddListener(OnHelpButtonClicked);
        }

        public void Initialize(TutorialManager tutorial)
        {
            tutorialManager = tutorial;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1))
            {
                OnHelpButtonClicked();
            }
        }

        private void OnHelpButtonClicked()
        {
            if (tutorialManager != null)
            {
                tutorialManager.StartTutorial();
            }
        }
    }
}
