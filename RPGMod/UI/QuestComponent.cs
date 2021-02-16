using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod {
namespace UI {
class QuestComponent : MonoBehaviour {
    private float progressVel;
    private GameObject questComponentUI;
    private Questing.QuestComponent questComponent;
    public int index;
    QuestComponent() {
        progressVel = 0.0f;

        // questComponent
        questComponentUI = new GameObject();
        questComponentUI.AddComponent<RectTransform>();

        questComponentUI.transform.SetParent(this.transform);

        questComponentUI.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        questComponentUI.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        questComponentUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        questComponentUI.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);

        //description
        GameObject description = new GameObject();

        description.name = "description";

        description.AddComponent<RectTransform>();
        description.AddComponent<TextMeshProUGUI>();

        description.transform.SetParent(questComponentUI.transform);

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

        progress.transform.SetParent(questComponentUI.transform);

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

        progressBarBackground.transform.SetParent(description.transform);

        progressBarBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        progressBarBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        progressBarBackground.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        progressBarBackground.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5, 0);
        progressBarBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(questComponentUI.GetComponent<RectTransform>().sizeDelta.x - 20, 8);

        progressBarBackground.GetComponent<Image>().color = new Color(0,0,0,0.7f);

        UI.Utils.AddBorder(progressBarBackground);

        // progressBar
        GameObject progressBar = new GameObject();

        progressBar.name = "progressBar";

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
    public void UpdateData(Questing.QuestComponent questComponent, int i) {
        this.questComponent = questComponent;
        questComponentUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-55 * i), 0);
    }
    private void Update() {
        questComponentUI.transform.Find("description").GetComponent<TextMeshProUGUI>().text = Quest.questTypeDict[questComponent.questType];
        questComponentUI.transform.Find("description").GetComponent<RectTransform>().sizeDelta = questComponentUI.transform.Find("description").GetComponent<TextMeshProUGUI>().GetPreferredValues();
        questComponentUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().text = String.Format("{0}/{1}", questComponent.Progress, questComponent.objective);
        questComponentUI.transform.Find("progress").GetComponent<RectTransform>().sizeDelta = questComponentUI.transform.Find("progress").GetComponent<TextMeshProUGUI>().GetPreferredValues();

        questComponentUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale = new Vector3(Mathf.SmoothDamp(questComponentUI.transform.Find("description").Find("progressBarBackground").Find("progressBar").transform.localScale.x, (float)questComponent.Progress/(float)questComponent.objective, ref progressVel, 0.7f),1,1);
    }
    public void Destroy() {
        Destroy(questComponentUI);
    }
}

} // namespace UI
} // namespace RPGMod