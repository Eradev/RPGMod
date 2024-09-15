using RoR2;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    public class Announcer : MonoBehaviour
    {
        private readonly GameObject _announcerUI;
        private readonly GameObject _announcerText;
        private bool _finished;
        private float _startTime;
        private float _vel;
        private float _vel2;
        private float _targetY;
        private float _targetAlpha;
        private readonly float _fadeTime;

        private Announcer()
        {
            _finished = false;
            _vel = 0;
            _vel2 = 0;
            _targetY = Utils.ScreenSize.y * ConfigValues.UI.AnnouncerPositionY;
            _targetAlpha = 1;
            _fadeTime = 0.7f;

            //announcerUI
            _announcerUI = new GameObject();

            _announcerUI.AddComponent<RectTransform>();
            _announcerUI.AddComponent<Image>();
            _announcerUI.AddComponent<CanvasGroup>();

            _announcerUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            _announcerUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            _announcerUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            _announcerUI.GetComponent<RectTransform>().sizeDelta = new Vector2(Utils.ScreenSize.x * ConfigValues.UI.AnnouncerScaleX, 100);

            _announcerUI.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.9f);

            _announcerUI.GetComponent<CanvasGroup>().alpha = 0;
            Utils.AddBorder(_announcerUI);

            // announcerText
            _announcerText = new GameObject();

            _announcerText.AddComponent<RectTransform>();
            _announcerText.AddComponent<TextMeshProUGUI>();

            _announcerText.transform.SetParent(_announcerUI.transform);

            _announcerText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            _announcerText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            _announcerText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            _announcerText.GetComponent<RectTransform>().sizeDelta = _announcerUI.GetComponent<RectTransform>().sizeDelta;
            _announcerText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

            _announcerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            _announcerText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            _announcerText.GetComponent<TextMeshProUGUI>().fontSize = 24;
            _announcerText.GetComponent<TextMeshProUGUI>().margin = new Vector4(10, 10, 10, 10);
            _announcerText.GetComponent<TextMeshProUGUI>().text = "";

            _announcerUI.transform.SetParent(transform);
            _announcerUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            _announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -Utils.ScreenSize.y * 0.03f, 0);
            _announcerUI.GetComponent<RectTransform>().localScale = new Vector3(Utils.HudScale, Utils.HudScale, Utils.HudScale);
        }

        public void SetMessage(string message)
        {
            StartCoroutine(PlayText(message));
        }

        private void Update()
        {

            _announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, Mathf.SmoothDamp(_announcerUI.GetComponent<RectTransform>().anchoredPosition.y, _targetY, ref _vel, _fadeTime), 0);
            _announcerUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(_announcerUI.GetComponent<CanvasGroup>().alpha, _targetAlpha, ref _vel2, _fadeTime);

            if (_finished && Run.instance.GetRunStopwatch() - _startTime >= _fadeTime + 0.5f)
            {
                Destroy();
            }
        }

        private IEnumerator PlayText(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '<')
                {
                    while (text[i] != '>' && i < text.Length - 1)
                    {
                        _announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                        i++;
                    }
                }

                Util.PlaySound("Play_UI_ChatMessage", RoR2Application.instance.gameObject);
                _announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                if (i < text.Length - 1 && (text[i + 1] == ' ' || text[i + 1] == '.' || text[i + 1] == ','))
                {
                    i++;
                    _announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                    yield return new WaitForSeconds(0.085f);
                }
                else
                {
                    yield return new WaitForSeconds(0.035f);
                }
            }
            yield return new WaitForSeconds(5);

            _startTime = Run.instance.GetRunStopwatch();
            _targetAlpha = 0;
            _targetY = -30;
            _finished = true;
        }

        public void Destroy()
        {
            Destroy(_announcerUI);
            Destroy(this);
        }
    }
}