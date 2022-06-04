using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RPGMod.UI
{
    public class Announcer : MonoBehaviour
    {
        private readonly GameObject announcerUI;
        private readonly GameObject announcerText;
        private bool finished;
        private float startTime;
        private float vel;
        private float vel2;
        private float targetY;
        private float targetAlpha;
        private readonly float fadeTime;

        private Announcer()
        {
            finished = false;
            vel = 0;
            vel2 = 0;
            targetY = Utils.screenSize.y * Config.UI.announcerPositionY;
            targetAlpha = 1;
            fadeTime = 0.7f;

            //announcerUI
            announcerUI = new GameObject();

            announcerUI.AddComponent<RectTransform>();
            announcerUI.AddComponent<Image>();
            announcerUI.AddComponent<CanvasGroup>();

            announcerUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            announcerUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            announcerUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            announcerUI.GetComponent<RectTransform>().sizeDelta = new Vector2(Utils.screenSize.x * Config.UI.announcerScaleX, 100);

            announcerUI.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.9f);

            announcerUI.GetComponent<CanvasGroup>().alpha = 0;
            Utils.AddBorder(announcerUI);

            // announcerText
            announcerText = new GameObject();

            announcerText.AddComponent<RectTransform>();
            announcerText.AddComponent<TextMeshProUGUI>();

            announcerText.transform.SetParent(announcerUI.transform);

            announcerText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            announcerText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            announcerText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            announcerText.GetComponent<RectTransform>().sizeDelta = announcerUI.GetComponent<RectTransform>().sizeDelta;
            announcerText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

            announcerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            announcerText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            announcerText.GetComponent<TextMeshProUGUI>().fontSize = 24;
            announcerText.GetComponent<TextMeshProUGUI>().margin = new Vector4(10, 10, 10, 10);
            announcerText.GetComponent<TextMeshProUGUI>().text = "";

            announcerUI.transform.SetParent(transform);
            announcerUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -Utils.screenSize.y * 0.03f, 0);
            announcerUI.GetComponent<RectTransform>().localScale = new Vector3(Utils.HudScale, Utils.HudScale, Utils.HudScale);
        }

        public void SetMessage(string message)
        {
            StartCoroutine(PlayText(message));
        }

        private void Update()
        {

            announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, Mathf.SmoothDamp(announcerUI.GetComponent<RectTransform>().anchoredPosition.y, targetY, ref vel, fadeTime), 0);
            announcerUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(announcerUI.GetComponent<CanvasGroup>().alpha, targetAlpha, ref vel2, fadeTime);

            if (finished && Run.instance.GetRunStopwatch() - startTime >= fadeTime + 0.5f)
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
                        announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                        i++;
                    }
                }

                Util.PlaySound("Play_UI_ChatMessage", RoR2Application.instance.gameObject);
                announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                if (i < text.Length - 1 && (text[i + 1] == ' ' || text[i + 1] == '.' || text[i + 1] == ','))
                {
                    i++;
                    announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                    yield return new WaitForSeconds(0.085f);
                }
                else
                {
                    yield return new WaitForSeconds(0.035f);
                }
            }
            yield return new WaitForSeconds(5);

            startTime = Run.instance.GetRunStopwatch();
            targetAlpha = 0;
            targetY = -30;
            finished = true;
        }

        public void Destroy()
        {
            Destroy(announcerUI);
            Destroy(this);
        }
    }
}