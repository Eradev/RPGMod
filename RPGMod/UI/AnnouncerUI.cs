using RoR2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RPGMod
{
public class AnnouncerUI : MonoBehaviour {
    private GameObject announcerUI;
    private GameObject announcerText;
    private bool attached;
    private bool finished = false;
    private float startTime;
    private float vel = 0;
    private float vel2 = 0;
    private float targetY = 30;
    private float targetAlpha = 1;
    private float fadeTime = 0.7f;
    private void Awake() {
        announcerUI = new GameObject();

        announcerUI.AddComponent<RectTransform>();
        announcerUI.AddComponent<Image>();
        announcerUI.AddComponent<CanvasGroup>();
        announcerUI.AddComponent<CanvasRenderer>();

        announcerUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        announcerUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        announcerUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        announcerUI.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * Config.UI.announcerScaleX, 100);

        announcerUI.GetComponent<Image>().color = new Color(0.16f,0.16f,0.16f,0.9f);

        announcerUI.GetComponent<CanvasGroup>().alpha = 0;

        announcerText = new GameObject();

        announcerText.AddComponent<RectTransform>();
        announcerText.AddComponent<TextMeshProUGUI>();
        announcerText.AddComponent<CanvasRenderer>();

        announcerText.transform.SetParent(announcerUI.transform);

        announcerText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        announcerText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        announcerText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        announcerText.GetComponent<RectTransform>().sizeDelta = announcerUI.GetComponent<RectTransform>().sizeDelta;
        announcerText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        announcerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        announcerText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        announcerText.GetComponent<TextMeshProUGUI>().fontSize = 24;
        announcerText.GetComponent<TextMeshProUGUI>().margin = new Vector4(10,10,10,10);
        announcerText.GetComponent<TextMeshProUGUI>().text = "";
    }
    public void SetMessage(String message) {
        StartCoroutine(PlayText(message));
    }
    private void Update() {
        if (!attached) {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser?.cameraRigController?.hud?.mainContainer != null)
            {
                announcerUI.transform.SetParent(localUser.cameraRigController.hud.mainContainer.transform);
                announcerUI.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
                announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -30, 0);
                announcerUI.GetComponent<RectTransform>().localScale = new Vector3(UI.hudScale,UI.hudScale,UI.hudScale);

                attached = true;

            }
        }
        else {
            announcerUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, Mathf.SmoothDamp(announcerUI.GetComponent<RectTransform>().anchoredPosition.y, targetY, ref vel, fadeTime), 0);
            announcerUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(announcerUI.GetComponent<CanvasGroup>().alpha, targetAlpha, ref vel2, fadeTime);
            if (finished) {
                if (Run.instance.GetRunStopwatch() - startTime >= fadeTime + 0.5f) {
                    Destroy(this);
                }
            }
        }
    }
    IEnumerator PlayText(String text)
	{
        for (int i = 0; i < text.Length; i++)  {
            if (text[i] == '<') {
                while (text[i] != '>' && i < text.Length - 1) {
                    announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                    i++;
                }
            }

            Util.PlaySound("Play_UI_ChatMessage", RoR2Application.instance.gameObject);
            announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
            if (i < text.Length - 1 && (text[i+1] == ' ' || text[i+1] == '.' || text[i+1] == ',')) {
                i++;
                announcerText.GetComponent<TextMeshProUGUI>().text += text[i];
                yield return new WaitForSeconds (0.085f);
            }
            else {
                yield return new WaitForSeconds (0.035f);
            }
        }
        yield return new WaitForSeconds (5);
        startTime = Run.instance.GetRunStopwatch();
        targetAlpha = 0;
        targetY = -30;
        finished = true;
	}
    private void OnDestroy()
    {
        Destroy(announcerUI);
    }
}
} // namespace RPGMod