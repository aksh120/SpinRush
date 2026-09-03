using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Audio;
using SpinRush.Core;
using SpinRush.Gameplay;

namespace SpinRush.UI
{
    /// <summary>
    /// Displays the Arcade Game Over screen and Top 5 High Score Leaderboard
    /// when the player runs out of tokens/bankroll.
    /// Provides session statistics and instant "Insert Coin / Play Again" restart.
    /// </summary>
    public class GameOverModalController : MonoBehaviour
    {
        [Header("Session Stats")]
        private int _spinsPlayed = 0;
        private int _totalWon = 0;
        private int _highestWin = 0;

        [Header("UI Objects")]
        private GameObject _modalRoot;
        private Text _statsSummaryText;
        private Text _leaderboardText;
        private Button _restartButton;

        [Header("Controllers")]
        [SerializeField] private SlotMachineController slotController;
        [SerializeField] private WalletManager walletManager;
        [SerializeField] private AudioController audioController;

        private const string PrefKeyScore = "SpinRush_Score_";
        private const string PrefKeyName = "SpinRush_Name_";

        public int SpinsPlayed => _spinsPlayed;
        public int TotalWon => _totalWon;
        public int HighestWin => _highestWin;

        private void Awake()
        {
            if (slotController == null) slotController = FindObjectOfType<SlotMachineController>();
            if (walletManager == null) walletManager = FindObjectOfType<WalletManager>();
            if (audioController == null) audioController = FindObjectOfType<AudioController>();

            InitializeDefaultLeaderboard();
            BuildGameOverModalUI();
        }

        private void OnEnable()
        {
            if (slotController != null)
            {
                slotController.OnSpinEvaluated += HandleSpinEvaluated;
            }
        }

        private void OnDisable()
        {
            if (slotController != null)
            {
                slotController.OnSpinEvaluated -= HandleSpinEvaluated;
            }
        }

        private void HandleSpinEvaluated(SpinResult result)
        {
            _spinsPlayed++;
            if (result.IsWin)
            {
                _totalWon += result.Payout;
                if (result.Payout > _highestWin)
                {
                    _highestWin = result.Payout;
                }
            }
        }

        public void ShowGameOver()
        {
            int sessionScore = _totalWon + (_spinsPlayed * 50);
            SaveScoreToLeaderboard(sessionScore);

            if (_modalRoot != null)
            {
                _modalRoot.SetActive(true);
                UpdateStatsDisplay(sessionScore);
                UpdateLeaderboardDisplay();
            }

            if (audioController != null)
            {
                audioController.PlayNearMissSigh();
            }
        }

        private void UpdateStatsDisplay(int sessionScore)
        {
            if (_statsSummaryText != null)
            {
                _statsSummaryText.text =
                    $"<color=#FFD700>TOTAL SPINS SURVIVED:</color> {_spinsPlayed}\n" +
                    $"<color=#00E5FF>HIGHEST WIN:</color> {WalletManager.FormatRupees(_highestWin)}\n" +
                    $"<color=#39FF14>FINAL SCORE:</color> {sessionScore:N0} PTS";
            }
        }

        private void UpdateLeaderboardDisplay()
        {
            if (_leaderboardText == null) return;

            string table = "<b>★ TOP 5 ARCADE CHAMPIONS ★</b>\n\n";
            for (int i = 0; i < 5; i++)
            {
                string pName = PlayerPrefs.GetString(PrefKeyName + i, "VIP");
                int pScore = PlayerPrefs.GetInt(PrefKeyScore + i, (5 - i) * 2000);
                string medal = (i == 0) ? "🥇" : (i == 1) ? "🥈" : (i == 2) ? "🥉" : $"#{i + 1}";
                table += $"{medal}  {pName,-6}  {pScore,8:N0} PTS\n";
            }
            _leaderboardText.text = table;
        }

        private void SaveScoreToLeaderboard(int newScore)
        {
            List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < 5; i++)
            {
                string n = PlayerPrefs.GetString(PrefKeyName + i, "VIP");
                int s = PlayerPrefs.GetInt(PrefKeyScore + i, (5 - i) * 2000);
                scores.Add(new KeyValuePair<string, int>(n, s));
            }

            scores.Add(new KeyValuePair<string, int>("YOU", newScore));
            scores.Sort((a, b) => b.Value.CompareTo(a.Value));

            for (int i = 0; i < 5; i++)
            {
                PlayerPrefs.SetString(PrefKeyName + i, scores[i].Key);
                PlayerPrefs.SetInt(PrefKeyScore + i, scores[i].Value);
            }
            PlayerPrefs.Save();
        }

        private void InitializeDefaultLeaderboard()
        {
            if (!PlayerPrefs.HasKey(PrefKeyScore + "0"))
            {
                string[] defaultNames = { "RAJ", "VIP", "LKY", "ACE", "PRO" };
                int[] defaultScores = { 18500, 12000, 8500, 5000, 2500 };
                for (int i = 0; i < 5; i++)
                {
                    PlayerPrefs.SetString(PrefKeyName + i, defaultNames[i]);
                    PlayerPrefs.SetInt(PrefKeyScore + i, defaultScores[i]);
                }
                PlayerPrefs.Save();
            }
        }

        public void RestartGame()
        {
            _spinsPlayed = 0;
            _totalWon = 0;
            _highestWin = 0;

            if (_modalRoot != null) _modalRoot.SetActive(false);

            if (walletManager != null)
            {
                walletManager.ResetBalance(GameConstants.DefaultStartingBalance);
            }

            if (audioController != null)
            {
                audioController.PlayWinCelebration(25f, false);
            }
        }

        private void BuildGameOverModalUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("GameOverModalRoot");
            if (existing != null) DestroyImmediate(existing.gameObject);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Modal Root (Dim backdrop)
            _modalRoot = new GameObject("GameOverModalRoot", typeof(RectTransform), typeof(Image));
            _modalRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = _modalRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            _modalRoot.GetComponent<Image>().color = new Color(0f, 0f, 0.05f, 0.88f);

            // Dialog Frame
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            card.transform.SetParent(_modalRoot.transform, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(500f, 430f);
            card.GetComponent<Image>().color = new Color(0.05f, 0.02f, 0.14f, 0.98f);

            Outline cOut = card.GetComponent<Outline>();
            cOut.effectColor = new Color(0.95f, 0.2f, 0.2f, 0.95f); // Crimson Arcade border
            cOut.effectDistance = new Vector2(3f, -3f);

            // Header Plaque
            GameObject hObj = new GameObject("Header", typeof(RectTransform), typeof(Text), typeof(Shadow));
            hObj.transform.SetParent(card.transform, false);
            RectTransform hRect = hObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, 175f);
            hRect.sizeDelta = new Vector2(460f, 40f);
            Text hTxt = hObj.GetComponent<Text>();
            hTxt.font = font;
            hTxt.text = "★ GAME OVER ★";
            hTxt.fontSize = 24;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.25f, 0.25f);

            // Subheader
            GameObject subObj = new GameObject("Subheader", typeof(RectTransform), typeof(Text));
            subObj.transform.SetParent(card.transform, false);
            RectTransform subRect = subObj.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0f, 140f);
            subRect.sizeDelta = new Vector2(460f, 25f);
            Text subTxt = subObj.GetComponent<Text>();
            subTxt.font = font;
            subTxt.text = "OUT OF CREDITS & TOKENS!";
            subTxt.fontSize = 13;
            subTxt.fontStyle = FontStyle.Bold;
            subTxt.alignment = TextAnchor.MiddleCenter;
            subTxt.color = new Color(0.85f, 0.85f, 0.9f);

            // Stats Summary
            GameObject statsObj = new GameObject("StatsSummary", typeof(RectTransform), typeof(Text), typeof(Shadow));
            statsObj.transform.SetParent(card.transform, false);
            RectTransform sRect = statsObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0f, 75f);
            sRect.sizeDelta = new Vector2(440f, 70f);
            _statsSummaryText = statsObj.GetComponent<Text>();
            _statsSummaryText.font = font;
            _statsSummaryText.fontSize = 13;
            _statsSummaryText.fontStyle = FontStyle.Bold;
            _statsSummaryText.alignment = TextAnchor.MiddleCenter;
            _statsSummaryText.color = Color.white;

            // Leaderboard Text
            GameObject lbObj = new GameObject("Leaderboard", typeof(RectTransform), typeof(Text), typeof(Shadow));
            lbObj.transform.SetParent(card.transform, false);
            RectTransform lbRect = lbObj.GetComponent<RectTransform>();
            lbRect.anchoredPosition = new Vector2(0f, -40f);
            lbRect.sizeDelta = new Vector2(440f, 130f);
            _leaderboardText = lbObj.GetComponent<Text>();
            _leaderboardText.font = font;
            _leaderboardText.fontSize = 12;
            _leaderboardText.alignment = TextAnchor.MiddleCenter;
            _leaderboardText.color = new Color(0.9f, 0.9f, 0.7f);

            // Restart Button
            GameObject btnObj = new GameObject("Btn_Restart", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            btnObj.transform.SetParent(card.transform, false);
            RectTransform bRect = btnObj.GetComponent<RectTransform>();
            bRect.anchoredPosition = new Vector2(0f, -165f);
            bRect.sizeDelta = new Vector2(300f, 48f);
            btnObj.GetComponent<Image>().color = new Color(0f, 0.7f, 0.45f); // Emerald

            Outline bOut = btnObj.GetComponent<Outline>();
            bOut.effectColor = new Color(1f, 0.9f, 0.4f);
            bOut.effectDistance = new Vector2(2f, -2f);

            GameObject btObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btObj.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;
            Text btTxt = btObj.GetComponent<Text>();
            btTxt.font = font;
            btTxt.text = "INSERT COIN / PLAY AGAIN";
            btTxt.fontSize = 15;
            btTxt.fontStyle = FontStyle.Bold;
            btTxt.alignment = TextAnchor.MiddleCenter;
            btTxt.color = Color.white;

            _restartButton = btnObj.GetComponent<Button>();
            _restartButton.onClick.AddListener(RestartGame);

            _modalRoot.SetActive(false);
        }
    }
}
