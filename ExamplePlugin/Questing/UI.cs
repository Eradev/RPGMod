using System.Collections.Generic;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod.Questing
{
    /// <summary>
    /// Controller for the UI used for each quest.
    /// </summary>
    public class UI : MonoBehaviour
    {
        private GameObject questUI;
        private Transform parent;
        private float startTime;
        private float progressStartTime;
        private bool animFinished = true;
        private bool validLocalUser;

        private float newSizeX;
        private float oldSizeX;
        private float hudScale;
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
        private int objective;
        private int progress;

        public static List<UI> Instances { get; private set; } = new List<UI>();
        public string questDataDescription;
        public int index;
        public Color questColor;

        public Color QuestColor
        {
            get => questColor;
            set
            {
                backgroundBorder.GetComponent<Image>().color = value;
                iconBorder.GetComponent<Image>().color = value;
                var barColor = value;
                barColor.a = 0.92f;
                progressBarPrimary.GetComponent<Image>().color = barColor;
                questColor = value;
            }
        }

        public int Index
        {
            get => index;
            set
            {
                index = value;

                var x = Screen.width * Config.XPositionUI;
                var y = Screen.height * Config.YPositionUI - questRect.sizeDelta.y * index * hudScale;
                var z = parent.position.z * 1.35f;

                questRect.transform.localPosition = new Vector3(x, y, z);
            }
        }

        public Texture ObjectiveIcon
        {
            get => objectiveIconHolder.GetComponent<RawImage>().texture;
            set => objectiveIconHolder.GetComponent<RawImage>().texture = value;
        }

        public Texture EquipIcon
        {
            get => equipIconHolder.GetComponent<RawImage>().texture;
            set => equipIconHolder.GetComponent<RawImage>().texture = value;
        }

        public Color EquipBorder
        {
            get => equipIconBorder.GetComponent<Image>().color;
            set => equipIconBorder.GetComponent<Image>().color = value;
        }

        public string Title
        {
            get => titleField.GetComponent<TextMeshProUGUI>().text;
            set => titleField.GetComponent<TextMeshProUGUI>().text = value;
        }

        public string Description
        {
            get => descriptionField.GetComponent<TextMeshProUGUI>().text;
            set => descriptionField.GetComponent<TextMeshProUGUI>().text = value;
        }

        public string Reward
        {
            get => rewardField.GetComponent<TextMeshProUGUI>().text;
            set => rewardField.GetComponent<TextMeshProUGUI>().text = value;
        }

        public string Progress
        {
            get => progressField.GetComponent<TextMeshProUGUI>().text;
            set => progressField.GetComponent<TextMeshProUGUI>().text = value;
        }

        public string QuestDataDescription
        {
            get => questDataDescription;
            set
            {
                var data = value.Split(',');
                //for (int i = 0; i < data.Length; i++)
                //{
                //    Debug.Log(data[i] + i);
                //}
                Title = Core.QuestDefinitions.Types[int.Parse(data[0])];
                QuestColor = Core.QuestDefinitions.Colors[int.Parse(data[0])];
                Description = data[1];
                Reward = data[2];
                progress = int.Parse(data[3]);
                objective = int.Parse(data[4]);
                Progress = $"{progress}/{objective}";
                oldSizeX = secondaryBarTransform.sizeDelta.x;
                newSizeX = 180f * (progress / (float)objective);
                StartProgressAnim();
                EquipIcon = Resources.Load<Texture>(data[5]);
                questDataDescription = value;
            }
        }

        // Methods
        private void Awake()
        {
            questUI = Instantiate(Core.AssetBundle.LoadAsset<GameObject>("Assets/QuestUI.prefab"));

            var hudConVar = Console.instance.FindConVar("hud_scale");

            if (Config.UseGameHudScale)
            {
                if (hudConVar != null && TextSerialization.TryParseInvariant(hudConVar.GetString(), out float num))
                {
                    hudScale = num / 100f;
                }
            }
            else
            {
                hudScale = 1f;
            }

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
            var num = (Time.time - progressStartTime) / 0.6f;
            if (num < 1)
            {
                secondaryBarTransform.sizeDelta = new Vector2(Mathf.SmoothStep(oldSizeX, newSizeX, num), primaryBarTransform.sizeDelta.y);
            }
            num = (Time.time - (progressStartTime + 0.2f)) / 0.8f;
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
            if (!validLocalUser)
            {
                var localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser.cameraRigController.hud.objectivePanelController != null)
                {
                    parent = localUser.cameraRigController.hud.GetComponent<Canvas>().transform;
                    questUI.transform.SetParent(parent);
                    validLocalUser = true;
                    questUI.transform.localScale = new Vector3(hudScale, hudScale, hudScale);
                }
            }

            if (!animFinished)
            {
                UpdateProgressAnim();
            }

            if (questUI == null)
            {
                Destroy(this);
                return;
            }

            var num = (Time.time - startTime) / 0.8f;

            if (num < 1)
            {
                var x = Mathf.SmoothStep(Screen.width * 1.3f, Screen.width * Config.XPositionUI, num);
                var y = Screen.height * Config.YPositionUI - questRect.sizeDelta.y * index * hudScale;
                var z = parent.position.z * 1.35f;

                questRect.transform.localPosition = new Vector3(x, y, z);
            }
        }

        private void OnDestroy()
        {
            Destroy(questUI);
        }
    }
}