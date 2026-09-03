using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Gameplay;

namespace SpinRush.UI
{
    /// <summary>
    /// Displays the luxury 3D neon arcade marquee logo "SPINRUSH — ROYAL VIP ARCADE"
    /// across the top crown of the slot machine cabinet with dynamic neon illumination.
    /// </summary>
    public class CabinetMarqueeHeader : MonoBehaviour
    {
        [Header("Marquee Settings")]
        [SerializeField] private Vector2 marqueePosition = new Vector2(45f, 218f);
        [SerializeField] private Vector2 marqueeSize = new Vector2(380f, 56f);

        private RectTransform _marqueeRoot;
        private Text _titleText;
        private Text _subtitleText;
        private Outline _neonOutline;

        private void Awake()
        {
            BuildMarqueeUI();
        }

        private void Start()
        {
            StartCoroutine(AnimateNeonShimmer());
        }

        private void BuildMarqueeUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Remove existing marquee if any
            Transform existing = canvas.transform.Find("CabinetMarqueeHeader");
            if (existing != null) DestroyImmediate(existing.gameObject);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Marquee Root Plaque
            GameObject rootObj = new GameObject("CabinetMarqueeHeader", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            rootObj.transform.SetParent(canvas.transform, false);
            _marqueeRoot = rootObj.GetComponent<RectTransform>();

            // Center marquee dynamically aligned with slot machine cabinet
            float posX = marqueePosition.x;
            SlotMachineController slotCtrl = FindObjectOfType<SlotMachineController>();
            if (slotCtrl != null)
            {
                RectTransform slotRect = slotCtrl.GetComponent<RectTransform>();
                if (slotRect != null) posX = slotRect.anchoredPosition.x;
            }
            _marqueeRoot.anchoredPosition = new Vector2(posX, marqueePosition.y);
            _marqueeRoot.sizeDelta = marqueeSize;

            Image bgImg = rootObj.GetComponent<Image>();
            bgImg.color = new Color(0.05f, 0.02f, 0.16f, 0.96f); // Deep Obsidian Glass

            _neonOutline = rootObj.GetComponent<Outline>();
            _neonOutline.effectColor = new Color(1f, 0.82f, 0.2f, 0.95f); // Radiant Gold Neon
            _neonOutline.effectDistance = new Vector2(2f, -2f);

            Shadow bgShadow = rootObj.GetComponent<Shadow>();
            bgShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            bgShadow.effectDistance = new Vector2(3f, -3f);

            // Title: "SPINRUSH"
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
            titleObj.transform.SetParent(rootObj.transform, false);
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchoredPosition = new Vector2(0f, 8f);
            tr.sizeDelta = new Vector2(360f, 36f);

            _titleText = titleObj.GetComponent<Text>();
            _titleText.font = font;
            _titleText.text = "SPINRUSH";
            _titleText.fontSize = 28;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = new Color(1f, 0.92f, 0.45f); // Brilliant Champagne Gold

            Outline tOutline = titleObj.GetComponent<Outline>();
            tOutline.effectColor = new Color(0.6f, 0.35f, 0.05f, 0.9f);
            tOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Shadow tShadow = titleObj.GetComponent<Shadow>();
            tShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            tShadow.effectDistance = new Vector2(2f, -2f);

            // Subtitle: "★ ROYAL VIP ARCADE ★"
            GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(Text), typeof(Shadow));
            subObj.transform.SetParent(rootObj.transform, false);
            RectTransform sr = subObj.GetComponent<RectTransform>();
            sr.anchoredPosition = new Vector2(0f, -16f);
            sr.sizeDelta = new Vector2(360f, 18f);

            _subtitleText = subObj.GetComponent<Text>();
            _subtitleText.font = font;
            _subtitleText.text = "★ ROYAL VIP ARCADE ★";
            _subtitleText.fontSize = 11;
            _subtitleText.fontStyle = FontStyle.Bold;
            _subtitleText.alignment = TextAnchor.MiddleCenter;
            _subtitleText.color = new Color(0f, 0.90f, 1f, 0.95f); // Electric Cyan Neon

            Shadow sShadow = subObj.GetComponent<Shadow>();
            sShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            sShadow.effectDistance = new Vector2(1f, -1f);
        }

        private IEnumerator AnimateNeonShimmer()
        {
            float timer = 0f;
            Color goldA = new Color(1f, 0.92f, 0.45f);
            Color goldB = new Color(1f, 0.75f, 0.20f);
            Color cyanA = new Color(0f, 0.90f, 1f);
            Color cyanB = new Color(0.5f, 1f, 0.8f);

            while (true)
            {
                timer += Time.deltaTime * 2.5f;
                float pulse = 0.5f + 0.5f * Mathf.Sin(timer);

                if (_titleText != null)
                {
                    _titleText.color = Color.Lerp(goldA, goldB, pulse);
                }

                if (_subtitleText != null)
                {
                    _subtitleText.color = Color.Lerp(cyanA, cyanB, pulse);
                }

                if (_neonOutline != null)
                {
                    float alpha = Mathf.Lerp(0.75f, 1f, pulse);
                    _neonOutline.effectColor = new Color(1f, 0.82f, 0.2f, alpha);
                }

                yield return null;
            }
        }
    }
}
