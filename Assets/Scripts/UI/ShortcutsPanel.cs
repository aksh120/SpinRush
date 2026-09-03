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
