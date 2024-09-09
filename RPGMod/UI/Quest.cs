using RoR2;
using RPGMod.Questing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    public class Quest : MonoBehaviour
    {
        public static readonly Dictionary<MissionType, string> QuestTypeDict = new Dictionary<MissionType, string>
        {
            { MissionType.KillAny, "Kill any enemies" },
            { MissionType.KillAnyBuff, "Kill any buffed elite enemies" },
            { MissionType.KillCommon, "Kill common enemies" },
            { MissionType.KillElite, "Kill elite enemies" },
            { MissionType.KillChampion, "Kill champion enemies" },
            { MissionType.KillFlying, "Kill flying enemies" },
            { MissionType.KillRed, "Kill Blazing enemies" },
            { MissionType.KillHaunted, "Kill Celestine enemies" },
            { MissionType.KillWhite, "Kill Glacial enemies" },
            { MissionType.KillPoison, "Kill Malachite enemies" },
            { MissionType.KillBlue, "Kill Overloading enemies" },
            { MissionType.KillLunar, "Kill Perfected enemies" },
            { MissionType.KillEarthDLC1, "Kill Mending enemies" },
            { MissionType.KillVoidDLC1, "Kill Voidtouched enemies" },
            { MissionType.KillAurelioniteDLC2, "Kill Gilded enemies" },
            { MissionType.KillBeadDLC2, "Kill Twisted enemies" },
            { MissionType.CollectGold, "Collect gold" }
        };

        private readonly GameObject _questUI;
        private readonly GameObject _questTitle;
        private readonly GameObject _rewardBacking;
        private readonly GameObject _rewardText;
        private readonly GameObject _reward;
        private float _backgroundVel;
        private readonly float _fadeTime;
        private float _startTime;
        private float _moveVel;
        private float _alphaVel;
        private float _targetX;
        private float _targetAlpha;
        private List<Mission> _questComponents;
        private bool _finished;
        private UIState _state;
        private QuestData _clientData;

        private Quest()
        {
            _targetX = Utils.ScreenSize.x * Config.UI.QuestPositionX;
            _backgroundVel = 0.0f;
            _fadeTime = 0.7f;
            _finished = false;
            _moveVel = 0;
            _alphaVel = 0;
            _targetAlpha = 1;
            _state = UIState.Creating;

            // questUI
            _questUI = new GameObject();

            _questUI.AddComponent<RectTransform>();
            _questUI.AddComponent<Image>();
            _questUI.AddComponent<CanvasGroup>();

            _questUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            _questUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            _questUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

            _questUI.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.9f);

            _questUI.GetComponent<CanvasGroup>().alpha = 0;

            Utils.AddBorder(_questUI);

            // questTitle
            _questTitle = new GameObject();

            _questTitle.AddComponent<RectTransform>();
            _questTitle.AddComponent<TextMeshProUGUI>();

            _questTitle.transform.SetParent(_questUI.transform);

            _questTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            _questTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            _questTitle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            _questTitle.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

            _questTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            _questTitle.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            _questTitle.GetComponent<TextMeshProUGUI>().fontSize = 38;
            _questTitle.GetComponent<TextMeshProUGUI>().text = "QUEST";

            _questTitle.GetComponent<RectTransform>().sizeDelta = _questTitle.GetComponent<TextMeshProUGUI>().GetPreferredValues();

            // rewardBacking
            _rewardBacking = new GameObject();

            _rewardBacking.AddComponent<RectTransform>();
            _rewardBacking.AddComponent<Image>();

            _rewardBacking.transform.SetParent(_questTitle.transform);

            _rewardBacking.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            _rewardBacking.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            _rewardBacking.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            _rewardBacking.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);
            _rewardBacking.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 65);

            _rewardBacking.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            // rewardBackground
            var rewardBackground = new GameObject();

            rewardBackground.AddComponent<RectTransform>();
            rewardBackground.AddComponent<Image>();

            rewardBackground.transform.SetParent(_rewardBacking.transform);

            rewardBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            rewardBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            rewardBackground.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            rewardBackground.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            rewardBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 72);

            rewardBackground.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);

            // reward
            _reward = new GameObject();

            _reward.AddComponent<RectTransform>();
            _reward.AddComponent<RawImage>();

            _reward.transform.SetParent(rewardBackground.transform);

            _reward.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            _reward.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            _reward.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            _reward.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            _reward.GetComponent<RectTransform>().sizeDelta = rewardBackground.GetComponent<RectTransform>().sizeDelta;

            Utils.AddBorder(_reward);

            // rewardText
            _rewardText = new GameObject();

            _rewardText.AddComponent<RectTransform>();
            _rewardText.AddComponent<TextMeshProUGUI>();

            _rewardText.transform.SetParent(_reward.transform);

            _rewardText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            _rewardText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            _rewardText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            _rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);

            _rewardText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            _rewardText.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            _rewardText.GetComponent<TextMeshProUGUI>().fontSize = 20;
        }

        public void UpdateData(QuestData newQuestData)
        {
            _clientData = newQuestData;
        }

        private void Update()
        {
            if (_clientData == null)
            {
                return;
            }

            switch (_state)
            {
                case UIState.Creating:
                    var pickupDef = PickupCatalog.GetPickupDef(_clientData!.Reward);

                    if (pickupDef == null)
                    {
                        RpgMod.Log.LogWarning($"PickupDef {_clientData!.Reward} is null.");

                        return;
                    }

                    _rewardText.GetComponent<TextMeshProUGUI>().text = Language.GetString(pickupDef.nameToken);
                    _rewardText.GetComponent<RectTransform>().sizeDelta = _rewardText.GetComponent<TextMeshProUGUI>().GetPreferredValues();
                    _rewardText.GetComponent<TextMeshProUGUI>().color = pickupDef.baseColor;
                    _reward.GetComponent<RawImage>().texture = pickupDef.iconTexture;

                    _questComponents = new List<Mission>();
                    for (var i = 0; i < _clientData.QuestComponents.Count; i++)
                    {
                        var questComponent = _rewardText.AddComponent<Mission>();
                        questComponent.Index = i;
                        _questComponents.Add(questComponent);
                    }

                    _questUI.transform.SetParent(transform);
                    _questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, _questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + _rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * _clientData.QuestComponents.Count) + _rewardText.GetComponent<RectTransform>().sizeDelta.y);
                    _questUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    _questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Utils.ScreenSize.x * Config.UI.QuestPositionX + 60, Utils.ScreenSize.y * Config.UI.QuestPositionY);
                    _questUI.GetComponent<RectTransform>().localScale = new Vector3(Utils.HudScale, Utils.HudScale, Utils.HudScale);

                    _state = UIState.Updating;

                    break;

                case UIState.Updating:

                    if (_clientData!.Complete && !_finished)
                    {
                        _startTime = Run.instance.GetRunStopwatch();
                        _targetAlpha = 0;
                        _targetX = Utils.ScreenSize.x * Config.UI.QuestPositionX + 60;
                        _finished = true;
                    }

                    for (var i = _questComponents.Count - 1; i >= 0; i--)
                    {
                        _questComponents[i].UpdateData(_clientData.QuestComponents[_questComponents[i].Index], i);

                        if (!_clientData.QuestComponents[_questComponents[i].Index].IsCompleted)
                        {
                            continue;
                        }

                        _questComponents[i].Destroy();
                        Destroy(_questComponents[i]);
                        _questComponents.RemoveAt(i);
                    }

                    var questUiRectTransform = _questUI.GetComponent<RectTransform>();

                    _questUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.SmoothDamp(questUiRectTransform.anchoredPosition.x, _targetX, ref _moveVel, _fadeTime), Utils.ScreenSize.y * Config.UI.QuestPositionY);
                    _questUI.GetComponent<CanvasGroup>().alpha = Mathf.SmoothDamp(_questUI.GetComponent<CanvasGroup>().alpha, _targetAlpha, ref _alphaVel, _fadeTime);
                    _questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.SmoothDamp(questUiRectTransform.sizeDelta.y, _questTitle.GetComponent<RectTransform>().sizeDelta.y + 13.5f + _rewardBacking.GetComponent<RectTransform>().sizeDelta.y + (55 * _questComponents.Count) + _rewardText.GetComponent<RectTransform>().sizeDelta.y, ref _backgroundVel, 0.7f));
                    _questUI.transform.Find("border").GetComponent<RectTransform>().sizeDelta = new Vector2(questUiRectTransform.sizeDelta.x + 2, questUiRectTransform.sizeDelta.y + 2);

                    break;
            }

            if (_finished && Run.instance.GetRunStopwatch() - _startTime >= _fadeTime + 0.5f)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            Destroy(_questUI);
            Destroy(this);
        }
    }
}