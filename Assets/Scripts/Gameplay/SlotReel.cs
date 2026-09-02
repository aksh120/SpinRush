using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpinRush.Core;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Physics-based controller for an individual reel column.
    /// Manages continuous infinite wrap-around scrolling, ease-in acceleration,
    /// high-speed spinning, and elastic bounce-back deceleration snapping to RNG outcomes.
    /// </summary>
    public class SlotReel : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [Tooltip("Index of this reel (0 = Left, 1 = Center, 2 = Right).")]
        [SerializeField] private int reelIndex = 0;

        [Tooltip("Reference to the global symbol database.")]
        [SerializeField] private SymbolDatabase symbolDatabase;

        [Tooltip("Vertical spacing between symbols in pixels.")]
        [SerializeField] private float symbolSpacing = GameConstants.SymbolHeight;

        [Tooltip("Maximum scrolling speed in pixels per second.")]
        [SerializeField] private float maxScrollSpeed = GameConstants.SpinScrollSpeed;

        [Header("Reel References")]
        [SerializeField] private RectTransform symbolsContainer;
        [SerializeField] private List<SlotSymbol> symbolSlots = new List<SlotSymbol>();

        [Header("State")]
        [SerializeField] private ReelSpinState spinState = ReelSpinState.Stopped;
        [SerializeField] private SymbolData currentCenterSymbol;

        // Internal motion tracking
        private float _currentSpeed = 0f;
        private Coroutine _motionCoroutine;
        private const float WrapThresholdBottom = -250f;
        private const float WrapThresholdTop = 250f;
        private const float TotalBufferHeight = 500f;

        public int ReelIndex => reelIndex;
        public ReelSpinState SpinState => spinState;
        public SymbolData CurrentCenterSymbol => currentCenterSymbol;
        public bool IsSpinning => spinState != ReelSpinState.Stopped;

        public event Action<int, SymbolData> OnReelStopped;

        private void Awake()
        {
            if (symbolsContainer == null)
            {
                symbolsContainer = GetComponent<RectTransform>();
            }

            if (symbolSlots == null || symbolSlots.Count == 0)
            {
                GetComponentsInChildren(true, symbolSlots);
            }
        }

        /// <summary>
        /// Initializes the reel with symbol slots and starting random symbols.
        /// </summary>
        public void Initialize(int index, SymbolDatabase db)
        {
            reelIndex = index;
            symbolDatabase = db;

            if (symbolSlots == null || symbolSlots.Count == 0)
            {
                GetComponentsInChildren(true, symbolSlots);
            }

            // Position initial 5 slots: +200, +100, 0, -100, -200
            float[] initialY = new float[] { 200f, 100f, 0f, -100f, -200f };
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                if (symbolSlots[i] != null)
                {
                    float yPos = i < initialY.Length ? initialY[i] : (2 - i) * symbolSpacing;
                    symbolSlots[i].RectTransform.anchoredPosition = new Vector2(0f, yPos);

                    if (symbolDatabase != null && symbolDatabase.Count > 0)
                    {
                        var sym = symbolDatabase.GetRandomSymbol();
                        symbolSlots[i].SetSymbol(sym);
                    }
                }
            }

            // Center slot is index 2 (at Y = 0)
            if (symbolSlots.Count > 2 && symbolSlots[2] != null)
            {
                currentCenterSymbol = symbolSlots[2].CurrentSymbol;
            }

            spinState = ReelSpinState.Stopped;
        }

        /// <summary>
        /// Starts the downward spin acceleration for this reel.
        /// </summary>
        public void StartSpin()
        {
            if (_motionCoroutine != null) StopCoroutine(_motionCoroutine);
            SetWinningHighlight(false);
            _motionCoroutine = StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            spinState = ReelSpinState.Accelerating;
            _currentSpeed = 0f;
            float accelRate = maxScrollSpeed * 2.5f;

            // Acceleration phase
            while (_currentSpeed < maxScrollSpeed && spinState == ReelSpinState.Accelerating)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, maxScrollSpeed, accelRate * Time.deltaTime);
                AdvanceSymbols(_currentSpeed * Time.deltaTime);
                yield return null;
            }

            spinState = ReelSpinState.Spinning;
            _currentSpeed = maxScrollSpeed;

            // Constant speed spin phase
            while (spinState == ReelSpinState.Spinning)
            {
                AdvanceSymbols(_currentSpeed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Moves all symbol slots downward by the given pixel delta, wrapping symbols from bottom to top.
        /// </summary>
        private void AdvanceSymbols(float deltaY)
        {
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                var slot = symbolSlots[i];
                if (slot == null) continue;

                Vector2 pos = slot.RectTransform.anchoredPosition;
                pos.y -= deltaY;

                // Infinite wrap-around
                if (pos.y < WrapThresholdBottom)
                {
                    pos.y += TotalBufferHeight;
                    if (symbolDatabase != null && symbolDatabase.Count > 0)
                    {
                        slot.SetSymbol(symbolDatabase.GetRandomSymbol());
                    }
                }

                slot.RectTransform.anchoredPosition = pos;
            }
        }

        /// <summary>
        /// Stops the reel at the specified pre-determined target symbol with an elastic bounce-back.
        /// </summary>
        public void StopAtTarget(SymbolData targetSymbol)
        {
            if (_motionCoroutine != null) StopCoroutine(_motionCoroutine);
            _motionCoroutine = StartCoroutine(DecelerateAndSnapRoutine(targetSymbol));
        }

        private IEnumerator DecelerateAndSnapRoutine(SymbolData targetSymbol)
        {
            spinState = ReelSpinState.Decelerating;
            currentCenterSymbol = targetSymbol;

            // Find the symbol currently highest up in the buffer (closest to +200) to assign target
            SlotSymbol landingSlot = null;
            float maxY = float.MinValue;
            foreach (var slot in symbolSlots)
            {
                if (slot != null && slot.RectTransform.anchoredPosition.y > maxY)
                {
                    maxY = slot.RectTransform.anchoredPosition.y;
                    landingSlot = slot;
                }
            }

            if (landingSlot != null && targetSymbol != null)
            {
                landingSlot.SetSymbol(targetSymbol);
            }

            // Smoothly move until landingSlot reaches Y = 0
            float remainingDistance = landingSlot != null ? landingSlot.RectTransform.anchoredPosition.y : 200f;
            if (remainingDistance < 100f) remainingDistance += TotalBufferHeight;

            float decelTimer = 0f;
            float decelDuration = 0.45f;
            float startDistance = remainingDistance;

            while (decelTimer < decelDuration)
            {
                decelTimer += Time.deltaTime;
                float t = Mathf.Clamp01(decelTimer / decelDuration);

                // Smooth cubic ease-out
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                float currentOffset = Mathf.Lerp(startDistance, -15f, ease); // slight overshoot
                float moveDelta = (startDistance - currentOffset) - (startDistance - remainingDistance);
                remainingDistance = currentOffset;

                AdvanceSymbols(moveDelta);
                yield return null;
            }

            // Snap back from overshoot to exact alignment (Y = 0) with spring-back
            spinState = ReelSpinState.Snapping;
            float snapTimer = 0f;
            float snapDuration = 0.15f;

            // Re-align all slots perfectly relative to landing slot at Y = 0
            if (landingSlot != null)
            {
                int landingIdx = symbolSlots.IndexOf(landingSlot);
                for (int i = 0; i < symbolSlots.Count; i++)
                {
                    int offsetFromLanding = (i - landingIdx + symbolSlots.Count) % symbolSlots.Count;
                    // offsets: 0 -> 0, 1 -> -100, 2 -> -200, 3 -> +200, 4 -> +100
                    float targetY = 0f;
                    switch (offsetFromLanding)
                    {
                        case 0: targetY = 0f; break;
                        case 1: targetY = -100f; break;
                        case 2: targetY = -200f; break;
                        case 3: targetY = 200f; break;
                        case 4: targetY = 100f; break;
                    }

                    Vector2 startPos = symbolSlots[i].RectTransform.anchoredPosition;
                    Vector2 endPos = new Vector2(0f, targetY);

                    // Quick elastic snap
                    symbolSlots[i].RectTransform.anchoredPosition = endPos;
                }
            }

            spinState = ReelSpinState.Stopped;
            _currentSpeed = 0f;
            _motionCoroutine = null;

            OnReelStopped?.Invoke(reelIndex, currentCenterSymbol);
        }

        /// <summary>
        /// Sets center symbol directly (used for immediate initialization).
        /// </summary>
        public void SetCenterSymbol(SymbolData symbol)
        {
            currentCenterSymbol = symbol;
            // Center slot is index 2
            if (symbolSlots.Count > 2 && symbolSlots[2] != null)
            {
                symbolSlots[2].SetSymbol(symbol);
                symbolSlots[2].RectTransform.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Highlights or unhighlights the winning symbol in the center position.
        /// </summary>
        public void SetWinningHighlight(bool active)
        {
            foreach (var slot in symbolSlots)
            {
                if (slot != null && Mathf.Abs(slot.RectTransform.anchoredPosition.y) < 20f)
                {
                    slot.SetHighlight(active);
                }
            }
        }
    }
}
