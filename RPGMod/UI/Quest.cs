using RoR2;
using System.Collections.Generic;
using RPGMod.Questing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    public class Quest : MonoBehaviour
    {
        public static readonly Dictionary<MissionType, string> QuestTypeDict = new Dictionary<MissionType, string>
        {
            { MissionType.killCommon, "Kill common enemies" },
            { MissionType.killElite, "Kill elite enemies" },
            { MissionType.killChampion, "Kill champion enemies" },
            { MissionType.killFlying, "Kill flying enemies" },
            { MissionType.killRed, "Kill Blazing enemies" },
            { MissionType.killHaunted, "Kill Celestine enemies" },
            { MissionType.killWhite, "Kill Glacial enemies" },
            { MissionType.killPoison, "Kill Malachite enemies" },
            { MissionType.killBlue, "Kill Overloading enemies" },
            { MissionType.killLunar, "Kill Perfected enemies" },
            { MissionType.killEarthDLC1, "Kill Mending enemies" },
            { MissionType.killVoidDLC1, "Kill Voidtouched enemies" },
            { MissionType.killByBackstab, "Kill enemies with a backstab" },
            { MissionType.collectGold, "Collect gold" }
        };

        private readonly GameObject questUI;
        private readonly GameObject questTitle;
        private readonly GameObject rewardBacking;
        private readonly GameObject rewardBackground;
        private readonly GameObject rewardText;
        private readonly GameObject reward;
        private float backgroundVel;
        private readonly float fadeTime;
        private float startTime;
        private float moveVel;
        private float alphaVel;
        private float targetX;
        private float targetAlpha;
        private List<Mission> questComponents;
        private bool finished;
        private UIState state;
        private QuestData clientData;

        private Quest()
        {
            targetX = Utils.screenSize.x * Config.UI.questPositionX;
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

            questUI.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.9f);

            questUI.GetComponent<CanvasGroup>().alpha = 0;

            Utils.AddBorder(questUI);

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

            rewardBacking.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

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

            rewardBackground.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);

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

            Utils.AddBorder(reward);

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

        public void UpdateData(QuestData newQuestData)
        {
            clientData = newQuestData;
        }

        private void Update()
        {
            if (clientData == null)
            {
                return;
            }

            switch (state)
            {
                case UIState.creating:

                    rewardText.GetComponent<TextMeshProUGUI>().text = Language.GetString(PickupCatalog.GetPickupDef(clientData.Reward).nameToken);
                    rewardText.GetComponent<RectTransform>().sizeDelta = rewardText.GetComponent<TextMeshProUGUI>().GetPreferredValues();
                    rewardText.GetComponent<TextMeshProUGUI>().color = PickupCatalog.GetPickupDef(clientData.Reward).baseColor;
                    reward.GetComponent<RawImage>().texture = PickupCatalog.GetPickupDef(clientData.Reward).iconTexture;

                    questComponents = new List<Mission>();
                    for (var i = 0; i < clientData.QuestComponents.Count; i++)
                    {
                        var questComponent = rewardText.AddComponent<Mission>();
                        questComponent.index = i;
                        questComponents.Add(questComponent);
                    }

                    questUI.transform.SetParent(transform);
                    questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * clientData.QuestComponents.Count) + rewardText.GetComponent<RectTransform>().sizeDelta.y);
                    questUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Utils.screenSize.x * Config.UI.questPositionX + 60, Utils.screenSize.y * Config.UI.questPositionY);
                    questUI.GetComponent<RectTransform>().localScale = new Vector3(Utils.HudScale, Utils.HudScale, Utils.HudScale);

                    state = UIState.updating;

                    break;

                case UIState.updating:

                    if ((bool)clientData?.Complete && !finished)
                    {
                        startTime = Run.instance.GetRunStopwatch();
                        targetAlpha = 0;
                        targetX = Utils.screenSize.x * Config.UI.questPositionX + 60;
                        finished = true;
                    }

                    for (var i = questComponents.Count - 1; i >= 0; i--)
                    {
                        questComponents[i].UpdateData(clientData.QuestComponents[questComponents[i].index], i);

                        if (!clientData.QuestComponents[questComponents[i].index].IsCompleted)
                        {
                            continue;
                        }

                        questComponents[i].Destroy();
                        Destroy(questComponents[i]);
                        questComponents.RemoveAt(i);
                    }

                    questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().anchoredPosition.x, targetX, ref moveVel, fadeTime), Utils.screenSize.y * Config.UI.questPositionY);
                    questUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(questUI.GetComponent<CanvasGroup>().alpha, targetAlpha, ref alphaVel, fadeTime);
                    questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().sizeDelta.y, questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * questComponents.Count) + rewardText.GetComponent<RectTransform>().sizeDelta.y, ref backgroundVel, 0.7f));
                    questUI.transform.Find("border").GetComponent<RectTransform>().sizeDelta = new Vector2(questUI.GetComponent<RectTransform>().sizeDelta.x + 2, questUI.GetComponent<RectTransform>().sizeDelta.y + 2);

                    break;
            }

            if (finished && Run.instance.GetRunStopwatch() - startTime >= fadeTime + 0.5f)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            Destroy(questUI);
            Destroy(this);
        }
    }
}