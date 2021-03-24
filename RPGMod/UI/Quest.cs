using RoR2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod
{
namespace UI {
public class Quest : MonoBehaviour {
    public static Dictionary<Questing.QuestType, String> questTypeDict = new Dictionary<Questing.QuestType, String>() {
        { Questing.QuestType.killCommon, "Kill common enemies" },
        { Questing.QuestType.killElite, "Kill elite enemies" },
        { Questing.QuestType.collectGold, "Collect gold" },
        { Questing.QuestType.attemptShrineChance, "Attempt chance shrines" }
    };
    private GameObject questUI;
    private GameObject questTitle;
    private GameObject rewardBacking;
    private GameObject rewardBackground;
    private GameObject rewardText;
    private GameObject reward;
    private float backgroundVel;
    private float fadeTime;
    private float startTime;
    private float moveVel;
    private float alphaVel;
    private float targetX;
    private float targetAlpha;
    private List<QuestComponent> questComponents;
    private bool finished;
    private UIState state;
    private Questing.QuestData clientData;
    Quest() {
        targetX = UI.Utils.screenSize.x * Config.UI.questPositionX;
        backgroundVel = 0.0f;
        fadeTime = 0.7f;
        finished = false;
        moveVel = 0;
        alphaVel = 0;
        targetAlpha = 1;
        state = UIState.creating;

        // questUI
        questUI = new GameObject();

        questUI.AddComponent<RectTransform>();
        questUI.AddComponent<Image>();
        questUI.AddComponent<CanvasGroup>();

        questUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        questUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        questUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        questUI.GetComponent<Image>().color = new Color(0.16f,0.16f,0.16f,0.9f);

        questUI.GetComponent<CanvasGroup>().alpha = 0;

        UI.Utils.AddBorder(questUI);

        // questTitle
        questTitle = new GameObject();

        questTitle.AddComponent<RectTransform>();
        questTitle.AddComponent<TextMeshProUGUI>();

        questTitle.transform.SetParent(questUI.transform);

        questTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        questTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        questTitle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        questTitle.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        questTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        questTitle.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        questTitle.GetComponent<TextMeshProUGUI>().fontSize = 38;
        questTitle.GetComponent<TextMeshProUGUI>().text = "QUEST";

        questTitle.GetComponent<RectTransform>().sizeDelta = questTitle.GetComponent<TextMeshProUGUI>().GetPreferredValues();

        // rewardBacking
        rewardBacking = new GameObject();

        rewardBacking.AddComponent<RectTransform>();
        rewardBacking.AddComponent<Image>();

        rewardBacking.transform.SetParent(questTitle.transform);

        rewardBacking.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        rewardBacking.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        rewardBacking.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        rewardBacking.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);
        rewardBacking.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 65);

        rewardBacking.GetComponent<Image>().color = new Color(0,0,0,0.5f);

        // rewardBackground
        rewardBackground = new GameObject();

        rewardBackground.AddComponent<RectTransform>();
        rewardBackground.AddComponent<Image>();

        rewardBackground.transform.SetParent(rewardBacking.transform);

        rewardBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        rewardBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        rewardBackground.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        rewardBackground.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        rewardBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 72);

        rewardBackground.GetComponent<Image>().color = new Color(0.16f,0.16f,0.16f);

        // reward
        reward = new GameObject();

        reward.AddComponent<RectTransform>();
        reward.AddComponent<RawImage>();

        reward.transform.SetParent(rewardBackground.transform);

        reward.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        reward.GetComponent<RectTransform>().sizeDelta = rewardBackground.GetComponent<RectTransform>().sizeDelta;

        UI.Utils.AddBorder(reward);

        // rewardText
        rewardText = new GameObject();

        rewardText.AddComponent<RectTransform>();
        rewardText.AddComponent<TextMeshProUGUI>();

        rewardText.transform.SetParent(reward.transform);

        rewardText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        rewardText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        rewardText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);

        rewardText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rewardText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        rewardText.GetComponent<TextMeshProUGUI>().fontSize = 20;
    }
    public void UpdateData(Questing.QuestData clientData) {
        this.clientData = clientData;
    }
    private void Update() {
        if (clientData == null) {
            return;
        }
        switch (state) {
            case UIState.creating:

                rewardText.GetComponent<TextMeshProUGUI>().text = Language.GetString(PickupCatalog.GetPickupDef(clientData.reward).nameToken);
                rewardText.GetComponent<RectTransform>().sizeDelta = rewardText.GetComponent<TextMeshProUGUI>().GetPreferredValues();
                rewardText.GetComponent<TextMeshProUGUI>().color = PickupCatalog.GetPickupDef(clientData.reward).baseColor;
                reward.GetComponent<RawImage>().texture = PickupCatalog.GetPickupDef(clientData.reward).iconTexture;

                questComponents = new List<QuestComponent>();
                for (int i = 0; i < clientData.questComponents.Count; i++) {
                    QuestComponent questComponent = rewardText.AddComponent<QuestComponent>();
                    questComponent.index = i;
                    questComponents.Add(questComponent);
                }

                questUI.transform.SetParent(this.transform);
                questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * clientData.questComponents.Count) + rewardText.GetComponent<RectTransform>().sizeDelta.y);
                questUI.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
                questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(UI.Utils.screenSize.x * Config.UI.questPositionX + 60, UI.Utils.screenSize.y * Config.UI.questPositionY);
                questUI.GetComponent<RectTransform>().localScale = new Vector3(UI.Utils.hudScale,UI.Utils.hudScale,UI.Utils.hudScale);

                state = UIState.updating;

                break;
            case UIState.updating:

                if ((clientData?.complete ?? false) && !finished) {
                    startTime = Run.instance.GetRunStopwatch();
                    targetAlpha = 0;
                    targetX = UI.Utils.screenSize.x * Config.UI.questPositionX + 60;
                    finished = true;
                }

                for (int i = questComponents.Count - 1; i >= 0; i--) {
                    questComponents[i].UpdateData(clientData.questComponents[questComponents[i].index], i);
                    if (clientData.questComponents[questComponents[i].index].complete) {
                        questComponents[i].Destroy();
                        Destroy(questComponents[i]);
                        questComponents.RemoveAt(i);
                    }
                }

                questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().anchoredPosition.x, targetX, ref moveVel, fadeTime), UI.Utils.screenSize.y * Config.UI.questPositionY);
                questUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(questUI.GetComponent<CanvasGroup>().alpha, targetAlpha, ref alphaVel, fadeTime);
                questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().sizeDelta.y, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * questComponents.Count) + rewardText.GetComponent<RectTransform>().sizeDelta.y, ref backgroundVel, 0.7f));
                questUI.transform.Find("border").GetComponent<RectTransform>().sizeDelta = new Vector2(questUI.GetComponent<RectTransform>().sizeDelta.x + 2, questUI.GetComponent<RectTransform>().sizeDelta.y + 2);

                break;
        }
        if (finished && Run.instance.GetRunStopwatch() - startTime >= fadeTime + 0.5f) {
            Destroy();
        }
    }
    public void Destroy() {
        Destroy(questUI);
        Destroy(this);
    }
}

} // namespace UI
} // namespace RPGMod