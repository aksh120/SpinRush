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
            if (helpButton != null)
            {
                helpButton.onClick.AddListener(OnHelpButtonClicked);
            }

            if (tutorialManager == null)
            {
                tutorialManager = FindObjectOfType<TutorialManager>();
            }

            RestylePanelAtRuntime();
        }

        private void RestylePanelAtRuntime()
        {
            // Background Chassis
            Image panelImg = GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.04f, 0.02f, 0.12f, 0.95f);
            }

            Outline panelOutline = GetComponent<Outline>();
            if (panelOutline == null) panelOutline = gameObject.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.85f);
            panelOutline.effectDistance = new Vector2(2f, -2f);

            Shadow panelShadow = GetComponent<Shadow>();
            if (panelShadow == null) panelShadow = gameObject.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0.05f, 0.15f, 0.7f);
            panelShadow.effectDistance = new Vector2(3f, -3f);

            // Iterate children to upgrade labels and keycaps
            Text[] allTexts = GetComponentsInChildren<Text>(true);
            foreach (var t in allTexts)
            {
                if (t.name == "Header" || t.text.Contains("CONTROL"))
                {
                    t.text = "ARCADE CONTROLS";
                    t.color = new Color(1f, 0.85f, 0.2f);
                    t.fontStyle = FontStyle.Bold;
                }
                else if (t.transform.parent != null && t.transform.parent.name.Contains("Key"))
                {
                    // Keycap text
                    t.color = Color.white;
                    t.fontStyle = FontStyle.Bold;
                    Image keyImg = t.transform.parent.GetComponent<Image>();
                    if (keyImg != null) keyImg.color = new Color(0.12f, 0.08f, 0.25f, 1f);
                    Outline keyOutline = t.transform.parent.GetComponent<Outline>();
                    if (keyOutline == null) keyOutline = t.transform.parent.gameObject.AddComponent<Outline>();
                    keyOutline.effectColor = new Color(0f, 0.85f, 1f, 0.8f); // Neon Cyan
                    keyOutline.effectDistance = new Vector2(1f, -1f);
                }
                else if (t.transform.parent != null && t.transform.parent.GetComponent<Button>() == null)
                {
                    // Action label
                    t.color = new Color(0.96f, 0.90f, 0.64f); // Champagne gold
                }
            }

            // Emerald Push Button
            if (helpButton != null)
            {
                Image hImg = helpButton.GetComponent<Image>();
                if (hImg != null) hImg.color = new Color(0f, 0.65f, 0.42f, 1f); // Vibrant emerald

                Outline hOutline = helpButton.GetComponent<Outline>();
                if (hOutline == null) hOutline = helpButton.gameObject.AddComponent<Outline>();
                hOutline.effectColor = new Color(0.3f, 1f, 0.6f, 0.9f);
                hOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Text hTxt = helpButton.GetComponentInChildren<Text>();
                if (hTxt != null)
                {
                    hTxt.text = "HOW TO PLAY";
                    hTxt.color = new Color(1f, 0.98f, 0.80f);
                    hTxt.fontStyle = FontStyle.Bold;
                }
            }
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
