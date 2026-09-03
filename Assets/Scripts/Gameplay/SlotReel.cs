using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpinRush.Core;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// High-performance slot reel controller managing continuous vertical scrolling,
    /// seamless infinite looping, cubic ease-out deceleration, and elastic bounce snapping.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlotReel : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [SerializeField] private int reelIndex = 0;
        [SerializeField] private float symbolHeight = 100f;
        [SerializeField] private float maxScrollSpeed = 2600f;

        [Header("Hierarchy References")]
        [SerializeField] private RectTransform stripTransform;
        [SerializeField] private List<SlotSymbol> stripSymbols = new List<SlotSymbol>();

        [Header("Runtime State")]
        [SerializeField] private ReelSpinState spinState = ReelSpinState.Stopped;
        [SerializeField] private SymbolData currentCenterSymbol;

        private RectTransform _rectTransform;
        private Coroutine _spinCoroutine;
        private Coroutine _decelCoroutine;
        private float _currentSpeed = 0f;
        private float _totalLoopHeight = 2000f;
        private SlotSymbol _activeLandedSlot;

        public int ReelIndex => reelIndex;
        public ReelSpinState SpinState => spinState;
        public SymbolData CurrentCenterSymbol => currentCenterSymbol;
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (stripTransform == null)
            {
                var child = transform.Find("SymbolsContainer");
                if (child != null) stripTransform = child.GetComponent<RectTransform>();
            }

            if (stripSymbols == null || stripSymbols.Count == 0)
            {
                GetComponentsInChildren(true, stripSymbols);
            }

            if (stripSymbols != null && stripSymbols.Count > 0)
            {
                _totalLoopHeight = stripSymbols.Count * symbolHeight;
            }
        }

        public void Initialize(int index, SymbolDatabase db)
        {
            reelIndex = index;
            _rectTransform = GetComponent<RectTransform>();

            if (stripTransform == null)
            {
                var child = transform.Find("SymbolsContainer");
                if (child != null) stripTransform = child.GetComponent<RectTransform>();
            }

            if (stripSymbols == null || stripSymbols.Count == 0)
            {
                GetComponentsInChildren(true, stripSymbols);
            }

            if (stripSymbols != null && stripSymbols.Count > 0)
            {
                _totalLoopHeight = stripSymbols.Count * symbolHeight;

                // Populate initial symbols if database is supplied
                if (db != null && db.Count > 0)
                {
                    for (int i = 0; i < stripSymbols.Count; i++)
                    {
                        var sym = db[i % db.Count];
                        stripSymbols[i].SetSymbol(sym);
                    }
                    currentCenterSymbol = stripSymbols[0].CurrentSymbol;
                    _activeLandedSlot = stripSymbols[0];
                }
            }

            if (stripTransform != null)
            {
                stripTransform.anchoredPosition = Vector2.zero;
            }

            spinState = ReelSpinState.Stopped;
        }

        public void SetStripReferences(RectTransform container, List<SlotSymbol> symbols)
        {
            stripTransform = container;
            stripSymbols = symbols;
            if (stripSymbols != null && stripSymbols.Count > 0)
            {
                _totalLoopHeight = stripSymbols.Count * symbolHeight;
                currentCenterSymbol = stripSymbols[0].CurrentSymbol;
                _activeLandedSlot = stripSymbols[0];
            }
        }

        /// <summary>
        /// Initiates continuous high-speed spinning.
        /// </summary>
        public void StartSpin()
        {
            if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
            if (_decelCoroutine != null) StopCoroutine(_decelCoroutine);

            SetWinningHighlight(false);
            _spinCoroutine = StartCoroutine(SpinLoopRoutine());
        }

        private IEnumerator SpinLoopRoutine()
        {
            spinState = ReelSpinState.Accelerating;
            _currentSpeed = 0f;

            // Anticipation wind-up: slight backward pull before firing down
            float windupDuration = 0.08f;
            float windupTimer = 0f;
            Vector2 initialPos = stripTransform != null ? stripTransform.anchoredPosition : Vector2.zero;

            while (windupTimer < windupDuration)
            {
                windupTimer += Time.deltaTime;
                float windupProgress = windupTimer / windupDuration;
                if (stripTransform != null)
                {
                    stripTransform.anchoredPosition = initialPos + new Vector2(0f, Mathf.Sin(windupProgress * Mathf.PI) * 12f);
                }
                yield return null;
            }

            // High-speed acceleration
            float accelDuration = 0.22f;
            float accelTimer = 0f;
            while (accelTimer < accelDuration)
            {
                accelTimer += Time.deltaTime;
                _currentSpeed = Mathf.Lerp(0f, maxScrollSpeed, accelTimer / accelDuration);
                AdvanceStrip(_currentSpeed * Time.deltaTime);
                yield return null;
            }

            spinState = ReelSpinState.Spinning;
            _currentSpeed = maxScrollSpeed;

            // Continuous spin loop
            while (spinState == ReelSpinState.Spinning)
            {
                AdvanceStrip(_currentSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void AdvanceStrip(float deltaY)
        {
            if (stripTransform == null) return;

            Vector2 pos = stripTransform.anchoredPosition;
            pos.y -= deltaY;

            // Seamless wrap-around modulo
            while (pos.y <= -_totalLoopHeight)
            {
                pos.y += _totalLoopHeight;
            }
            while (pos.y > 0f)
            {
                pos.y -= _totalLoopHeight;
            }

            stripTransform.anchoredPosition = pos;
        }

        /// <summary>
        /// Stops the reel smoothly at the designated target symbol with cubic deceleration and elastic bounce.
        /// </summary>
        public void StopAtTarget(SymbolData targetSymbol)
        {
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }

            if (_decelCoroutine != null) StopCoroutine(_decelCoroutine);
            _decelCoroutine = StartCoroutine(DecelerateAndSnapRoutine(targetSymbol));
        }

        private IEnumerator DecelerateAndSnapRoutine(SymbolData targetSymbol)
        {
            spinState = ReelSpinState.Decelerating;

            if (stripTransform == null || stripSymbols == null || stripSymbols.Count == 0)
            {
                spinState = ReelSpinState.Stopped;
                yield break;
            }

            // Find matching symbols on strip
            int bestTargetIndex = 0;
            List<int> matchingIndices = new List<int>();
            for (int i = 0; i < stripSymbols.Count; i++)
            {
                if (stripSymbols[i] != null && stripSymbols[i].CurrentSymbol != null)
                {
                    if (targetSymbol != null && stripSymbols[i].CurrentSymbol.SymbolId == targetSymbol.SymbolId)
                    {
                        matchingIndices.Add(i);
                    }
                }
            }

            if (matchingIndices.Count == 0)
            {
                // Fallback if not found: replace symbol at index 0
                bestTargetIndex = 0;
                stripSymbols[0].SetSymbol(targetSymbol);
            }
            else
            {
                // Pick the first matching index
                bestTargetIndex = matchingIndices[0];
            }

            // Center of symbol at index k is at Y = k * symbolHeight
            // To bring symbol k to Y = 0 in viewport, strip Y must be -k * symbolHeight
            float targetStripY = -bestTargetIndex * symbolHeight;

            // Current position
            float currentY = stripTransform.anchoredPosition.y;

            // We must move DOWN (decreasing Y). Calculate distance to targetStripY
            // Ensure at least 1 full loop (1.5x) of deceleration travel
            float diff = currentY - targetStripY;
            while (diff < _totalLoopHeight * 1.2f)
            {
                diff += _totalLoopHeight;
            }

            float startY = currentY;
            float endY = startY - diff; // final Y position before modulo wrap
            float decelDuration = 0.52f;
            float timer = 0f;

            // Phase 1: Cubic Ease-Out with Overshoot (-18px)
            float overshoot = 18f;
            while (timer < decelDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / decelDuration);
                // Cubic ease-out
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                float y = Mathf.Lerp(startY, endY - overshoot, ease);

                // Modulo wrap into visible range
                float wrappedY = y;
                while (wrappedY <= -_totalLoopHeight) wrappedY += _totalLoopHeight;
                while (wrappedY > 0f) wrappedY -= _totalLoopHeight;

                stripTransform.anchoredPosition = new Vector2(0f, wrappedY);
                yield return null;
            }

            // Phase 2: Elastic Bounce-Back to exact center
            float bounceDuration = 0.16f;
            float bounceTimer = 0f;
            float finalTargetY = targetStripY;
            while (finalTargetY <= -_totalLoopHeight) finalTargetY += _totalLoopHeight;
            while (finalTargetY > 0f) finalTargetY -= _totalLoopHeight;

            float overshootY = finalTargetY - overshoot;

            while (bounceTimer < bounceDuration)
            {
                bounceTimer += Time.deltaTime;
                float bt = Mathf.Clamp01(bounceTimer / bounceDuration);
                // Damped spring bounce
                float spring = Mathf.Sin(bt * Mathf.PI * 0.5f);
                float currentBounceY = Mathf.Lerp(overshootY, finalTargetY, spring);
                stripTransform.anchoredPosition = new Vector2(0f, currentBounceY);
                yield return null;
            }

            // Exact snap
            stripTransform.anchoredPosition = new Vector2(0f, finalTargetY);

            currentCenterSymbol = targetSymbol;
            _activeLandedSlot = stripSymbols[bestTargetIndex];
            spinState = ReelSpinState.Stopped;
            _decelCoroutine = null;
        }

        /// <summary>
        /// Activates or clears the winning payline highlight on the landed symbol.
        /// </summary>
        public void SetWinningHighlight(bool active)
        {
            if (_activeLandedSlot != null)
            {
                _activeLandedSlot.SetHighlight(active);
            }
        }
    }
}
