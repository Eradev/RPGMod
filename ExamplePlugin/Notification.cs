namespace RPGMod
{
    using RoR2;
    using RoR2.UI;
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UIController : MonoBehaviour
    {
        public GameObject questUI;
        public Transform parent;
        public float startTime;
        public RectTransform questRect;
        public RectTransform primaryBarTransform;
        public Transform iconBorder;
        public Transform backgroundBorder;
        public Transform infoBackground;
        public Transform objectiveIconHolder;
        public Transform equipIconBorder;
        public Transform equipIconHolder;
        public Transform progressBackground;
        public Transform progressField;
        public Transform titleField;
        public Transform progressBarPrimary;
        public Transform descriptionField;
        public Transform rewardField;
        public string questDataDescription;
        public int objective;
        public int progress;

        public int index;
        public Color questColor;
        public Color QuestColor { get { return questColor; } set {
                //backgroundBorder.GetComponent<Image>().color = value;
                //iconBorder.GetComponent<Image>().color = value;
                //progressBarPrimary.GetComponent<Image>().color = value;
                //questColor = value;
            } }
        public Texture ObjectiveIcon { get { return objectiveIconHolder.GetComponent<RawImage>().texture; } set { objectiveIconHolder.GetComponent<RawImage>().texture = value; } }
        public Texture EquipIcon { get { return equipIconHolder.GetComponent<RawImage>().texture; } set { equipIconHolder.GetComponent<RawImage>().texture = value; } }
        public String Title { get { return titleField.GetComponent<TextMeshProUGUI>().text; } set { titleField.GetComponent<TextMeshProUGUI>().text = value; } }
        public String Description { get { return descriptionField.GetComponent<TextMeshProUGUI>().text; } set { descriptionField.GetComponent<TextMeshProUGUI>().text = value; } }
        public String Reward { get { return rewardField.GetComponent<TextMeshProUGUI>().text; } set { rewardField.GetComponent<TextMeshProUGUI>().text = value; } }
        public String Progress { get { return progressField.GetComponent<TextMeshProUGUI>().text; } set {
                progressField.GetComponent<TextMeshProUGUI>().text = value;
                // float newSizeX = 180 * progress / objective;
                // primaryBarTransform.sizeDelta = new Vector2(newSizeX, primaryBarTransform.sizeDelta.y);
                // primaryBarTransform.position = new Vector3(-90 + (newSizeX / 2), primaryBarTransform.position.y, 0);
            } }

        public String QuestDataDescription { get { return questDataDescription;  } set {
                String[] data = value.Split(',');
                foreach (var i in data)
                {
                    Debug.Log(i);
                }
                Title = MainDefs.questTypes[int.Parse(data[0])];
                QuestColor = MainDefs.questColors[int.Parse(data[0])];
                Description = data[1];
                Reward = data[2];
                progress = int.Parse(data[3]);
                objective = int.Parse(data[4]);
                Progress = String.Format("{0}/{1}", progress, objective);
                EquipIcon = Resources.Load<Texture>(data[5]);
                questDataDescription = value;
            } }


        public void Awake()
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
            progressBarPrimary = infoBackground.Find("progressBarPrimary");
            progressField = progressBackground.Find("progress");

            Debug.Log("Loaded");
        }

        public void Start() {
            startTime = Time.time;
            questRect = questUI.GetComponent<RectTransform>();
            primaryBarTransform = progressBarPrimary.GetComponent<RectTransform>();
        }

        private void setProgressBar() {

        }

        public void Update()
        {

            if (questUI == null)
            {
                Destroy(this);
                return;
            } 

            float num = (Time.time - startTime) / 0.8f;
            if (num < 1)
            {
                questRect.position = new Vector3(Mathf.SmoothStep(Screen.width * 1.3f, Screen.width * ModConfig.screenPosX / 100f, num), (Screen.height * ModConfig.screenPosY / 100f) - ( questRect.sizeDelta.y * index ), 0);
            }
        }

        private void OnDestroy()
        {
            Destroy(questUI);
        }
    }
}