using RoR2;
using RPGMod.Extensions;
using RPGMod.Questing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    public class MissionComponent : MonoBehaviour
    {
        private float _progressVel;
        private readonly GameObject _missionUI;
        private Mission _mission;
        public int Index;

        private MissionComponent()
        {
            _progressVel = 0.0f;

            // mission
            _missionUI = new GameObject();
            _missionUI.AddComponent<RectTransform>();

            _missionUI.transform.SetParent(transform);

            _missionUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            _missionUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            _missionUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            _missionUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);

            // description
            var description = new GameObject
            {
                name = "description"
            };

            description.AddComponent<RectTransform>();
            description.AddComponent<TextMeshProUGUI>();

            description.transform.SetParent(_missionUI.transform);

            description.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            description.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            description.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            description.GetComponent<RectTransform>().anchoredPosition = new Vector3(10, -10, 0);

            description.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomLeft;
            description.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            description.GetComponent<TextMeshProUGUI>().fontSize = 16.5f;

            // progress
            var progress = new GameObject
            {
                name = "progress"
            };

            progress.AddComponent<RectTransform>();
            progress.AddComponent<TextMeshProUGUI>();

            progress.transform.SetParent(_missionUI.transform);

            progress.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            progress.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            progress.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            progress.GetComponent<RectTransform>().anchoredPosition = new Vector3(-10, -10, 0);

            progress.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
            progress.GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
            progress.GetComponent<TextMeshProUGUI>().fontSize = 16.5f;

            // progressBarBackground
            var progressBarBackground = new GameObject
            {
                name = "progressBarBackground"
            };

            progressBarBackground.AddComponent<RectTransform>();
            progressBarBackground.AddComponent<Image>();

            progressBarBackground.transform.SetParent(description.transform);

            progressBarBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            progressBarBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            progressBarBackground.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            progressBarBackground.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);
            progressBarBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(_missionUI.GetComponent<RectTransform>().sizeDelta.x - 20, 8);

            progressBarBackground.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            Utils.AddBorder(progressBarBackground);

            // progressBar
            var progressBar = new GameObject
            {
                name = "progressBar"
            };

            progressBar.AddComponent<RectTransform>();
            progressBar.AddComponent<Image>();

            progressBar.transform.SetParent(progressBarBackground.transform);

            progressBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            progressBar.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            progressBar.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            progressBar.GetComponent<RectTransform>().sizeDelta = progressBarBackground.GetComponent<RectTransform>().sizeDelta;
            progressBar.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
        }

        public void UpdateData(Mission mission, int i)
        {
            _mission = mission;
            _missionUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-55 * i), 0);
        }

        private void Update()
        {
            _missionUI.transform.Find("description").GetComponent<TextMeshProUGUI>().text = string.Format(Quest.QuestTypeDict[_mission.MissionType], Language.GetString(_mission.MissionSpecification).RemoveReplacementTokens());
            _missionUI.transform.Find("description").GetComponent<RectTransform>().sizeDelta = _missionUI.transform.Find("description").GetComponent<TextMeshProUGUI>().GetPreferredValues();
            _missionUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().text = $"{_mission.Progress}/{_mission.Objective}";
            _missionUI.transform.Find("progress").GetComponent<RectTransform>().sizeDelta = _missionUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().GetPreferredValues();

            _missionUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale = new Vector3(Mathf.SmoothDamp(_missionUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale.x, _mission.Progress / (float)_mission.Objective, ref _progressVel, 0.7f), 1, 1);
        }

        public void Destroy()
        {
            Destroy(_missionUI);
        }
    }
}