using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.UI
{
    public class Mission : MonoBehaviour
    {
        private float progressVel;
        private GameObject missionUI;
        private Questing.Mission mission;
        public int index;

        private Mission()
        {
            progressVel = 0.0f;

            // mission
            missionUI = new GameObject();
            missionUI.AddComponent<RectTransform>();

            missionUI.transform.SetParent(transform);

            missionUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            missionUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            missionUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            missionUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);

            // description
            var description = new GameObject
            {
                name = "description"
            };

            description.AddComponent<RectTransform>();
            description.AddComponent<TextMeshProUGUI>();

            description.transform.SetParent(missionUI.transform);

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

            progress.transform.SetParent(missionUI.transform);

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
            progressBarBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(missionUI.GetComponent<RectTransform>().sizeDelta.x - 20, 8);

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

        public void UpdateData(Questing.Mission mission, int i)
        {
            this.mission = mission;
            missionUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-55 * i), 0);
        }

        private void Update()
        {
            missionUI.transform.Find("description").GetComponent<TextMeshProUGUI>().text = Quest.QuestTypeDict[mission.MissionType];
            missionUI.transform.Find("description").GetComponent<RectTransform>().sizeDelta = missionUI.transform.Find("description").GetComponent<TextMeshProUGUI>().GetPreferredValues();
            missionUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().text = $"{mission.Progress}/{mission.Objective}";
            missionUI.transform.Find("progress").GetComponent<RectTransform>().sizeDelta = missionUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().GetPreferredValues();

            missionUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale = new Vector3(Mathf.SmoothDamp(missionUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale.x, mission.Progress / (float)mission.Objective, ref progressVel, 0.7f), 1, 1);
        }

        public void Destroy()
        {
            Destroy(missionUI);
        }
    }
}