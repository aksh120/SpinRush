using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Audio;

namespace SpinRush.UI
{
    /// <summary>
    /// Interactive step-by-step onboarding tutorial with dynamic element spotlight highlighting,
    /// animated dialogue card, Skip, Next, and 'Do not show again' persistence.
    /// Preserves authentic neon arcade styling.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [System.Serializable]
        public class TutorialStep
        {
            public string stepTitle;
            [TextArea(2, 4)] public string stepDescription;
            public RectTransform targetElement;
            public Vector2 dialogueOffset = new Vector2(0f, -220f);
        }

        [Header("UI Hierarchy")]
        [SerializeField] private GameObject tutorialRoot;
        [SerializeField] private RectTransform spotlightBox;
        [SerializeField] private Image spotlightGlowImage;
        [SerializeField] private RectTransform dialogueContainer;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text stepDotsText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Text nextButtonText;
        [SerializeField] private Button skipButton;
        [SerializeField] private Toggle doNotShowAgainToggle;

        [Header("Step Configuration")]
        [SerializeField] private TutorialStep[] steps;

        [Header("Audio")]
        [SerializeField] private AudioController audioController;

        private const string PrefKey_DoNotShow = "SpinRush_DoNotShowTutorial";
        private int _currentStepIndex = 0;
        private Coroutine _highlightCoroutine;
        private Coroutine _pulseCoroutine;

        private void Awake()
        {
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);
            if (doNotShowAgainToggle != null) doNotShowAgainToggle.onValueChanged.AddListener(OnDoNotShowToggleChanged);

            if (audioController == null) audioController = FindObjectOfType<AudioController>();
        }

        private void Start()
        {
            // Check if first-run tutorial should be shown
            int doNotShow = PlayerPrefs.GetInt(PrefKey_DoNotShow, 0);
            if (doNotShow == 0)
            {
                StartTutorial();
            }
            else
            {
                if (tutorialRoot != null) tutorialRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // Keyboard shortcut 'H' to reopen tutorial anytime
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (tutorialRoot != null && tutorialRoot.activeSelf)
                {
                    CloseTutorial();
                }
                else
                {
                    StartTutorial();
                }
            }

            // Space or Enter advances step if tutorial is open
            if (tutorialRoot != null && tutorialRoot.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnNextClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnSkipClicked();
                }
            }
        }

        public void Initialize(
            GameObject root,
            RectTransform spotlight,
            Image spotlightGlow,
            RectTransform dialogue,
            Text title,
            Text desc,
            Text stepDots,
            Button nextBtn,
            Text nextBtnTxt,
            Button skipBtn,
            Toggle doNotShowToggle,
            TutorialStep[] tutorialSteps,
            AudioController audio = null)
        {
            tutorialRoot = root;
            spotlightBox = spotlight;
            spotlightGlowImage = spotlightGlow;
            dialogueContainer = dialogue;
            titleText = title;
            descriptionText = desc;
            stepDotsText = stepDots;
            nextButton = nextBtn;
            nextButtonText = nextBtnTxt;
            skipButton = skipBtn;
            doNotShowAgainToggle = doNotShowToggle;
            steps = tutorialSteps;
            audioController = audio;

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (doNotShowAgainToggle != null)
            {
                doNotShowAgainToggle.onValueChanged.RemoveAllListeners();
                doNotShowAgainToggle.onValueChanged.AddListener(OnDoNotShowToggleChanged);
                doNotShowAgainToggle.isOn = PlayerPrefs.GetInt(PrefKey_DoNotShow, 0) == 1;
            }
        }

        public void StartTutorial()
        {
            _currentStepIndex = 0;
            if (tutorialRoot != null) tutorialRoot.SetActive(true);

            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(SpotlightPulseRoutine());

            ShowStep(_currentStepIndex);
        }

        private void ShowStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Length)
            {
                CloseTutorial();
                return;
            }

            TutorialStep step = steps[index];

            if (titleText != null) titleText.text = step.stepTitle;
            if (descriptionText != null) descriptionText.text = step.stepDescription;

            if (stepDotsText != null)
            {
                string dots = "";
                for (int i = 0; i < steps.Length; i++)
                {
                    dots += (i == index) ? "<color=#FFD700>●</color> " : "<color=#888888>○</color> ";
                }
                stepDotsText.text = dots.TrimEnd();
            }

            if (nextButtonText != null)
            {
                nextButtonText.text = (index == steps.Length - 1) ? "LETS PLAY!" : "NEXT >";
            }

            // Animate spotlight to surround target element
            if (step.targetElement != null && spotlightBox != null)
            {
                if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
                _highlightCoroutine = StartCoroutine(AnimateSpotlightTo(step.targetElement, step.dialogueOffset));
            }

            if (audioController != null) audioController.PlayButtonClick();
        }

        private IEnumerator AnimateSpotlightTo(RectTransform target, Vector2 dialogueOffset)
        {
            Vector3 targetWorldPos = target.position;
            Vector2 targetSize = target.rect.size;
            Vector3 targetScale = target.lossyScale;

            // If target is full cabinet (e.g. step 4), focus cleanly on the top marquee area!
            if (targetSize.y > 400f)
            {
                targetSize = new Vector2(430f, 130f);
                targetWorldPos.y += 195f * targetScale.y;
            }

            Vector2 scaledSize = new Vector2(targetSize.x * targetScale.x + 16f, targetSize.y * targetScale.y + 16f);

            Vector3 startPos = spotlightBox.position;
            Vector2 startSize = spotlightBox.sizeDelta;
            float duration = 0.28f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, timer / duration);
                spotlightBox.position = Vector3.Lerp(startPos, targetWorldPos, t);
                spotlightBox.sizeDelta = Vector2.Lerp(startSize, scaledSize, t);
                yield return null;
            }

            spotlightBox.position = targetWorldPos;
            spotlightBox.sizeDelta = scaledSize;

            // Position dialogue box comfortably relative to spotlight
            if (dialogueContainer != null)
            {
                Vector2 targetAnchored = spotlightBox.anchoredPosition + dialogueOffset;
                // Clamp within screen boundaries
                targetAnchored.x = Mathf.Clamp(targetAnchored.x, -500f, 500f);
                targetAnchored.y = Mathf.Clamp(targetAnchored.y, -320f, 320f);
                dialogueContainer.anchoredPosition = targetAnchored;
            }

            _highlightCoroutine = null;
        }

        private IEnumerator SpotlightPulseRoutine()
        {
            if (spotlightGlowImage != null)
            {
                // Clear transparent interior with ultra-subtle golden tint
                spotlightGlowImage.color = new Color(1f, 0.85f, 0.2f, 0.03f);
            }

            Outline outline = spotlightBox != null ? spotlightBox.GetComponent<Outline>() : null;

            while (true)
            {
                if (outline != null)
                {
                    // Pulse only the sleek outline border
                    float alpha = 0.60f + Mathf.Sin(Time.time * 5f) * 0.35f;
                    Color c = outline.effectColor;
                    c.a = alpha;
                    outline.effectColor = c;
                }
                yield return null;
            }
        }

        public void OnNextClicked()
        {
            _currentStepIndex++;
            if (_currentStepIndex < steps.Length)
            {
                ShowStep(_currentStepIndex);
            }
            else
            {
                CloseTutorial();
            }
        }

        public void OnSkipClicked()
        {
            if (audioController != null) audioController.PlayButtonClick();
            CloseTutorial();
        }

        private void OnDoNotShowToggleChanged(bool isOn)
        {
            PlayerPrefs.SetInt(PrefKey_DoNotShow, isOn ? 1 : 0);
            PlayerPrefs.Save();
            if (audioController != null) audioController.PlayButtonClick();
        }

        public void CloseTutorial()
        {
            if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);

            if (tutorialRoot != null) tutorialRoot.SetActive(false);

            if (doNotShowAgainToggle != null && doNotShowAgainToggle.isOn)
            {
                PlayerPrefs.SetInt(PrefKey_DoNotShow, 1);
                PlayerPrefs.Save();
            }
        }
    }
}
