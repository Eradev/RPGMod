using RoR2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod
{
public class QuestUI : MonoBehaviour {
    public static Dictionary<Questing.QuestType, String> questTypeDict = new Dictionary<Questing.QuestType, String>() {
        { Questing.QuestType.killCommon, "Kill common enemies" },
        { Questing.QuestType.killElite, "Kill elite enemies" },
        { Questing.QuestType.collectGold, "Collect gold" }
    };
    private GameObject questUI;
    private GameObject questTitle;
    private GameObject rewardBacking;
    private GameObject rewardBackground;
    private GameObject rewardText;
    private GameObject reward;
    private bool attached = false;
    private float backgroundVel = 0.0f;
    private float fadeTime = 0.7f;
    private bool finished = false;
    private float startTime;
    private float moveVel = 0;
    private float alphaVel = 0;
    private float targetX = Screen.width * Config.UI.questPositionX;
    private float targetAlpha = 1;
    private List<float> progressVels = new List<float>();
    private List<GameObject> questComponents;
    private List<int> indexes = new List<int>();
    private void Awake()
    {
        // questUI
        questUI = new GameObject();

        questUI.AddComponent<RectTransform>();
        questUI.AddComponent<Image>();
        questUI.AddComponent<CanvasGroup>();
        questUI.AddComponent<CanvasRenderer>();

        questUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        questUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        questUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        questUI.GetComponent<Image>().color = new Color(0.16f,0.16f,0.16f,0.9f);

        questUI.GetComponent<CanvasGroup>().alpha = 0;

        UI.AddBorder(questUI);
        // questTitle
        questTitle = new GameObject();

        questTitle.AddComponent<RectTransform>();
        questTitle.AddComponent<TextMeshProUGUI>();
        questTitle.AddComponent<CanvasRenderer>();

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
        rewardBacking.AddComponent<CanvasRenderer>();

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
        rewardBackground.AddComponent<CanvasRenderer>();

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
        reward.AddComponent<CanvasRenderer>();

        reward.transform.SetParent(rewardBackground.transform);

        reward.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        reward.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        reward.GetComponent<RectTransform>().sizeDelta = rewardBackground.GetComponent<RectTransform>().sizeDelta;

        UI.AddBorder(reward);
        // rewardText
        rewardText = new GameObject();

        rewardText.AddComponent<RectTransform>();
        rewardText.AddComponent<TextMeshProUGUI>();
        rewardText.AddComponent<CanvasRenderer>();

        rewardText.transform.SetParent(reward.transform);

        rewardText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        rewardText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        rewardText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);

        rewardText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rewardText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        rewardText.GetComponent<TextMeshProUGUI>().fontSize = 20;

        questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + 15);
        questComponents = new List<GameObject>();


    }
    private void Update() {
        if (!attached) {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser?.cameraRigController?.hud?.mainContainer != null)
            {
                questUI.transform.SetParent(localUser.cameraRigController.hud.mainContainer.transform);
                questUI.GetComponent<RectTransform>().localScale = new Vector3(UI.hudScale, UI.hudScale, UI.hudScale);
                questUI.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
                questUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(Screen.width * Config.UI.questPositionX + 60, Screen.height * Config.UI.questPositionY,0);

                attached = true;

            }
        }
    }
    public void UpdateData(Questing.PlayerData playerData) {


        if ((playerData == null || playerData.complete) && !finished) {
            startTime = Run.instance.GetRunStopwatch();
            targetAlpha = 0;
            targetX = Screen.width * Config.UI.questPositionX + 60;
            finished = true;
        }
        else {
            if (finished) {
                if (Run.instance.GetRunStopwatch() - startTime >= fadeTime + 0.5f) {
                    Destroy(this);
                }
            }

            rewardText.GetComponent<TextMeshProUGUI>().text = Language.GetString(PickupCatalog.GetPickupDef(playerData.reward).nameToken);
            rewardText.GetComponent<RectTransform>().sizeDelta = rewardText.GetComponent<TextMeshProUGUI>().GetPreferredValues();
            rewardText.GetComponent<TextMeshProUGUI>().color = PickupCatalog.GetPickupDef(playerData.reward).baseColor;
            reward.GetComponent<RawImage>().texture = PickupCatalog.GetPickupDef(playerData.reward).iconTexture;

            // Update QuestComponents
            int j = 0;
            for (int i = 0; i < playerData.questComponents.Count; i++) {
                if (!indexes.Contains(i) && !playerData.questComponents[i].complete) {
                    indexes.Add(i);
                    GameObject questComponent = new GameObject();
                    // questComponent
                    questComponent.AddComponent<RectTransform>();
                    questComponent.AddComponent<CanvasRenderer>();

                    questComponent.transform.SetParent(rewardText.transform);

                    questComponent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
                    questComponent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
                    questComponent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
                    questComponent.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
                    //description
                    GameObject description = new GameObject();

                    description.name = "description";

                    description.AddComponent<RectTransform>();
                    description.AddComponent<TextMeshProUGUI>();
                    description.AddComponent<CanvasRenderer>();

                    description.transform.SetParent(questComponent.transform);

                    description.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    description.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    description.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    description.GetComponent<RectTransform>().anchoredPosition = new Vector3(10, -10, 0);

                    description.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomLeft;
                    description.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
                    description.GetComponent<TextMeshProUGUI>().fontSize = 16.5f;
                    // progress
                    GameObject progress = new GameObject();

                    progress.name = "progress";

                    progress.AddComponent<RectTransform>();
                    progress.AddComponent<TextMeshProUGUI>();
                    progress.AddComponent<CanvasRenderer>();

                    progress.transform.SetParent(questComponent.transform);

                    progress.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                    progress.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                    progress.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    progress.GetComponent<RectTransform>().anchoredPosition = new Vector3(-10, -10, 0);

                    progress.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
                    progress.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
                    progress.GetComponent<TextMeshProUGUI>().fontSize = 16.5f;
                    // progressBarBackground
                    GameObject progressBarBackground = new GameObject();

                    progressBarBackground.name = "progressBarBackground";

                    progressBarBackground.AddComponent<RectTransform>();
                    progressBarBackground.AddComponent<Image>();
                    progressBarBackground.AddComponent<CanvasRenderer>();

                    progressBarBackground.transform.SetParent(description.transform);

                    progressBarBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                    progressBarBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                    progressBarBackground.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    progressBarBackground.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);
                    progressBarBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(questComponent.GetComponent<RectTransform>().sizeDelta.x - 20, 8);

                    progressBarBackground.GetComponent<Image>().color = new Color(0,0,0,0.7f);

                    UI.AddBorder(progressBarBackground);
                    // progressBar
                    GameObject progressBar = new GameObject();

                    progressBar.name = "progressBar";

                    progressBar.AddComponent<RectTransform>();
                    progressBar.AddComponent<Image>();
                    progressBar.AddComponent<CanvasRenderer>();

                    progressBar.transform.SetParent(progressBarBackground.transform);

                    progressBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    progressBar.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    progressBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    progressBar.GetComponent<RectTransform>().sizeDelta = progressBarBackground.GetComponent<RectTransform>().sizeDelta;
                    progressBar.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);

                    progressVels.Add(0.0f);
                    questComponents.Add(questComponent);
                }
                if (indexes.Contains(i)) {
                    if (playerData.questComponents[i].complete) {
                        Destroy(questComponents[j]);
                        questComponents.RemoveAt(j);
                        progressVels.RemoveAt(j);
                        indexes.Remove(i);
                    }
                    else {
                        questComponents[j].transform.Find("description").GetComponent<TextMeshProUGUI>().text = questTypeDict[playerData.questComponents[i].questType];
                        questComponents[j].transform.Find("description").GetComponent<RectTransform>().sizeDelta = questComponents[j].transform.Find("description").GetComponent<TextMeshProUGUI>().GetPreferredValues();
                        questComponents[j].transform.Find("progress").GetComponent<TextMeshProUGUI>().text = String.Format("{0}/{1}", playerData.questComponents[i].Progress, playerData.questComponents[i].objective);
                        questComponents[j].transform.Find("progress").GetComponent<RectTransform>().sizeDelta = questComponents[j].transform.Find("progress").GetComponent<TextMeshProUGUI>().GetPreferredValues();

                        float vel = progressVels[j];
                        questComponents[j].transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale = new Vector3(Mathf.SmoothDamp(questComponents[j].transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale.x, (float)playerData.questComponents[i].Progress/(float)playerData.questComponents[i].objective, ref vel, 0.7f),1,1);
                        progressVels[j] = vel;

                        questComponents[j].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-55 * j), 0);
                        j++;
                    }
                }
            }
            questUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().anchoredPosition.x, targetX, ref moveVel, fadeTime), Screen.height * Config.UI.questPositionY, 0);
            questUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(questUI.GetComponent<CanvasGroup>().alpha, targetAlpha, ref alphaVel, fadeTime);
            questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().sizeDelta.y, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * questComponents.Count) + 15, ref backgroundVel, 0.7f));
            questUI.transform.Find("border").GetComponent<RectTransform>().sizeDelta = new Vector2(questUI.GetComponent<RectTransform>().sizeDelta.x + 2, questUI.GetComponent<RectTransform>().sizeDelta.y + 2);
        }
    }
    private void OnDestroy()
    {
        Destroy(questUI);
    }
}
} // namespace RPGMod