using RoR2;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPGMod {
namespace Questing {


// Controller for the UI used for each quest.
public class UI : MonoBehaviour
{
    private GameObject questUI;
    private Transform parent;
    private float startTime;
    private float progressStartTime;
    private bool firstSet = true;
    private bool animFinished = true;
    //private bool destroy = false;
    private float newSizeX;
    private float oldSizeX;
    private RectTransform questRect;
    private RectTransform primaryBarTransform;
    private RectTransform secondaryBarTransform;
    private Transform iconBorder;
    private Transform backgroundBorder;
    private Transform infoBackground;
    private Transform objectiveIconHolder;
    private Transform equipIconBorder;
    private Transform equipIconHolder;
    private Transform progressBackground;
    private Transform progressField;
    private Transform titleField;
    private Transform progressBarPrimary;
    private Transform progressBarSecondary;
    private Transform descriptionField;
    private Transform rewardField;
    private string questDataDescription;
    private int objective;
    private int progress;

    public int index;
    public Color questColor;
    public Color QuestColor
    {
        get { return questColor; }
        set
        {
            backgroundBorder.GetComponent<Image>().color = value;
            iconBorder.GetComponent<Image>().color = value;
            Color barColor = value;
            barColor.a = 0.92f;
            progressBarPrimary.GetComponent<Image>().color = barColor;
            questColor = value;
        }
    }
    public int Index
    {
        get { return index; }
        set
        {
            index = value;
            questRect.position = new Vector3(Screen.width * Questing.Config.xPositionUI, (Screen.height * Questing.Config.yPositionUI) - (questRect.sizeDelta.y * index), 0);
        }
    }
    public Texture ObjectiveIcon { get { return objectiveIconHolder.GetComponent<RawImage>().texture; } set { objectiveIconHolder.GetComponent<RawImage>().texture = value; } }
    public Texture EquipIcon { get { return equipIconHolder.GetComponent<RawImage>().texture; } set { equipIconHolder.GetComponent<RawImage>().texture = value; } }
    public String Title { get { return titleField.GetComponent<TextMeshProUGUI>().text; } set { titleField.GetComponent<TextMeshProUGUI>().text = value; } }
    public String Description { get { return descriptionField.GetComponent<TextMeshProUGUI>().text; } set { descriptionField.GetComponent<TextMeshProUGUI>().text = value; } }
    public String Reward { get { return rewardField.GetComponent<TextMeshProUGUI>().text; } set { rewardField.GetComponent<TextMeshProUGUI>().text = value; } }
    public String Progress
    {
        get { return progressField.GetComponent<TextMeshProUGUI>().text; }
        set
        {
            progressField.GetComponent<TextMeshProUGUI>().text = value;
        }
    }

    public String QuestDataDescription
    {
        get { return questDataDescription; }
        set
        {
            String[] data = value.Split(',');
            for (int i = 0; i < data.Length; i++)
            {
                Debug.Log(data[i] + i);
            }
            Title = MainDefs.questTypes[int.Parse(data[0])];
            QuestColor = MainDefs.questColors[int.Parse(data[0])];
            Description = data[1];
            Reward = data[2];
            progress = int.Parse(data[3]);
            objective = int.Parse(data[4]);
            Progress = String.Format("{0}/{1}", progress, objective);
            oldSizeX = secondaryBarTransform.sizeDelta.x;
            newSizeX = 180f * ((float)progress / (float)objective);
            if (!firstSet)
            {
                StartProgressAnim();
            }
            else
            {
                firstSet = false;
            }
            EquipIcon = Resources.Load<Texture>(data[5]);
            questDataDescription = value;
        }
    }


    private void Awake()
    {
        parent = RoR2Application.instance.mainCanvas.transform;
        questUI = Instantiate(MainDefs.assetBundle.LoadAsset<GameObject>("Assets/QuestUI.prefab"));
        questUI.transform.SetParent(parent);

        iconBorder = questUI.transform.Find("iconBorder");
        backgroundBorder = questUI.transform.Find("backgroundBorder");
        infoBackground = questUI.transform.Find("background");
        objectiveIconHolder = iconBorder.Find("icon");
        titleField = infoBackground.Find("title");
        descriptionField = infoBackground.Find("description");
        rewardField = infoBackground.Find("reward");
        equipIconBorder = infoBackground.Find("equipIconBorder");
        equipIconHolder = equipIconBorder.Find("equipIcon");
        progressBackground = infoBackground.Find("progressBackground");
        progressBarPrimary = progressBackground.Find("progressBarPrimary");
        progressBarSecondary = progressBackground.Find("progressBarSecondary");
        progressField = progressBackground.Find("progress");

        primaryBarTransform = progressBarPrimary.GetComponent<RectTransform>();
        secondaryBarTransform = progressBarSecondary.GetComponent<RectTransform>();
    }

    private void Start()
    {
        startTime = Time.time;
        questRect = questUI.GetComponent<RectTransform>();
    }

    private void StartProgressAnim()
    {
        progressStartTime = Time.time;
        animFinished = false;
    }

    private void UpdateProgressAnim()
    {

        float num = (Time.time - progressStartTime) / 0.6f;
        if (num < 1)
        {
            secondaryBarTransform.sizeDelta = new Vector2(Mathf.SmoothStep(oldSizeX, newSizeX, num), primaryBarTransform.sizeDelta.y);
        }
        num = (Time.time - progressStartTime) / 1;
        if (num < 1)
        {
            primaryBarTransform.sizeDelta = new Vector2(Mathf.SmoothStep(oldSizeX, newSizeX, num), primaryBarTransform.sizeDelta.y);
        }
        else
        {
            animFinished = true;
        }

    }

    private void Update()
    {
        if (!animFinished)
        {
            UpdateProgressAnim();
        }

        if (questUI == null)
        {
            Destroy(this);
            return;
        }

        float num = (Time.time - startTime) / 0.8f;
        if (num < 1)
        {
            questRect.position = new Vector3(Mathf.SmoothStep(Screen.width * 1.3f, Screen.width * Questing.Config.xPositionUI, num), (Screen.height * Questing.Config.yPositionUI) - (questRect.sizeDelta.y * index), 0);
        }
    }

    private void OnDestroy()
    {
        Destroy(questUI);
        //destroy = true;
    }


}
}
}