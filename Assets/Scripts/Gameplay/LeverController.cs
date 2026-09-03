using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SpinRush.Audio;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Realistic physical mechanical slot machine lever.
    /// Features real-time mouse drag tracking, smooth cubic pull stroke,
    /// mechanical ratchet audio feedback, and damped harmonic spring-back oscillation.
    /// </summary>
    public class LeverController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Hierarchy References")]
        [Tooltip("Transform of the isolated lever arm sprite rotating around its hinge pivot.")]
        [SerializeField] private RectTransform leverArmTransform;

        [Tooltip("Full-cabinet overlay image for the pulled-down state (slot-machine3.png).")]
        [SerializeField] private Image leverDownOverlay;

        [Tooltip("Central slot machine controller.")]
        [SerializeField] private SlotMachineController controller;

        [Tooltip("Procedural audio controller for ratchet and spring clacks.")]
        [SerializeField] private AudioController audioController;

        [Header("Physics & Motion Settings")]
        [SerializeField] private float maxPullAngle = 65f;       // Degrees to rotate forward/down
        [SerializeField] private float pullThreshold = 0.35f;    // 35% drag triggers spin
        [SerializeField] private float springDuration = 0.40f;   // Return stroke duration

        private bool _isDragging = false;
        private bool _hasTriggeredSpin = false;
        private float _currentPullProgress = 0f; // 0 = upright, 1 = fully pulled down
        private Coroutine _animationCoroutine;
        private Vector2 _dragStartPos;

        private void Awake()
        {
            if (leverArmTransform == null)
            {
                leverArmTransform = GetComponent<RectTransform>();
            }

            if (controller == null)
            {
                controller = FindObjectOfType<SlotMachineController>();
            }

            if (audioController == null)
            {
                audioController = FindObjectOfType<AudioController>();
            }

            SetLeverPose(0f);
        }

        public void Initialize(RectTransform armRect, Image downOverlay, SlotMachineController slotController, AudioController audio = null)
        {
            leverArmTransform = armRect;
            leverDownOverlay = downOverlay;
            controller = slotController;
            audioController = audio;

            SetLeverPose(0f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller != null && controller.IsSpinning) return;

            _isDragging = true;
            _hasTriggeredSpin = false;
            _dragStartPos = eventData.position;

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

            // Audio click transient
            if (audioController != null) audioController.PlayLeverPull();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _hasTriggeredSpin) return;

            // Track vertical mouse drag downwards (negative Y)
            float deltaY = _dragStartPos.y - eventData.position.y;
            float dragRange = 180f; // 180px drag for full stroke
            _currentPullProgress = Mathf.Clamp01(deltaY / dragRange);

            SetLeverPose(_currentPullProgress);

            // If dragged past trigger threshold, fire the spin!
            if (_currentPullProgress >= pullThreshold && !_hasTriggeredSpin)
            {
                TriggerSpinFromLever();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (!_hasTriggeredSpin)
            {
                // If clicked without dragging, or pulled slightly: trigger full auto-pull animation!
                if (_currentPullProgress < 0.15f)
                {
                    AnimateFullPullAndRelease();
                }
                else
                {
                    // Released early without reaching threshold: spring back to top
                    StartSpringReturn(0f);
                }
            }
        }

        /// <summary>
        /// Smooth animated pull stroke when clicked directly.
        /// </summary>
        public void AnimateFullPullAndRelease()
        {
            if (controller != null && controller.IsSpinning) return;

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(FullPullRoutine());
        }

        private IEnumerator FullPullRoutine()
        {
            _hasTriggeredSpin = false;
            float pullTime = 0.18f;
            float timer = 0f;

            // Phase 1: Smooth quadratic pull down
            while (timer < pullTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / pullTime);
                float progress = t * t; // ease-in
                SetLeverPose(progress);
                yield return null;
            }

            SetLeverPose(1f);
            TriggerSpinFromLever();

            // Hold at bottom briefly to convey weight
            yield return new WaitForSeconds(0.08f);

            // Phase 2: Damped spring return
            yield return StartCoroutine(SpringReturnRoutine());
        }

        private void TriggerSpinFromLever()
        {
            _hasTriggeredSpin = true;

            if (audioController != null) audioController.PlayLeverPull();

            if (controller != null)
            {
                controller.RequestSpin();
            }

            if (!_isDragging)
            {
                // Start return stroke
                StartSpringReturn(1f);
            }
        }

        private void StartSpringReturn(float startProgress)
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(SpringReturnRoutine(startProgress));
        }

        private IEnumerator SpringReturnRoutine(float startProgress = 1f)
        {
            float elapsed = 0f;
            float duration = springDuration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Damped harmonic oscillation spring formula:
                // Decays from 1 to 0 with slight overshoot bounce
                float decay = Mathf.Exp(-t * 6f);
                float oscillation = Mathf.Cos(t * Mathf.PI * 3.5f);
                float progress = Mathf.Lerp(startProgress, 0f, 1f - decay * oscillation);

                SetLeverPose(Mathf.Clamp01(progress));
                yield return null;
            }

            SetLeverPose(0f);
            _hasTriggeredSpin = false;
            _animationCoroutine = null;
        }

        /// <summary>
        /// Updates the visual angle, perspective scale, and overlay visibility for the given progress (0..1).
        /// </summary>
        private void SetLeverPose(float progress)
        {
            _currentPullProgress = progress;

            if (leverArmTransform != null)
            {
                // Rotate forward and compress Y scale to simulate 3D downward perspective pull
                float angle = -progress * maxPullAngle;
                leverArmTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

                float yScale = Mathf.Lerp(1f, 0.45f, progress);
                float xScale = Mathf.Lerp(1f, 1.12f, progress);
                leverArmTransform.localScale = new Vector3(xScale, yScale, 1f);
            }

            // At maximum pull (progress > 0.85), activate the bottom ball overlay
            if (leverDownOverlay != null)
            {
                leverDownOverlay.gameObject.SetActive(progress > 0.85f);
            }
        }
    }
}
