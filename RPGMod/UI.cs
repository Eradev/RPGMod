using RoR2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMod
{
public class UI : MonoBehaviour {
    public static AssetBundle assetBundle;
    public static Dictionary<Questing.QuestType, String> questTypeDict = new Dictionary<Questing.QuestType, String>() {
        { Questing.QuestType.killCommon, "Kill common enemies" },
        { Questing.QuestType.killElite, "Kill elite enemies" },
        { Questing.QuestType.collectGold, "Collect gold" }
    };
    private GameObject questUI;
    private float origHeight;
    private bool attached = false;
    private float backgroundVel = 0.0f;
    private List<float> progressVels = new List<float>();
    private float hudScale;
    private List<GameObject> questComponents;
    private List<int> indexes = new List<int>();
    private void Awake()
    {
        questUI = Instantiate(assetBundle.LoadAsset<GameObject>("Assets/QuestUI.prefab"));
        questUI.transform.Find("QuestCanvas").Find("Title").GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        questUI.transform.Find("QuestCanvas").Find("RewardBorder").Find("RewardText").GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
        origHeight = questUI.GetComponent<RectTransform>().sizeDelta.y;
        questComponents = new List<GameObject>();

        var hudConVar = RoR2.Console.instance.FindConVar("hud_scale");

        if (Config.UI.useHUDScale)
        {
            if (hudConVar != null && TextSerialization.TryParseInvariant(hudConVar.GetString(), out float num))
            {
                hudScale = num / 100f;
            }
        }
        else {
            hudScale = 1f;
        }
    }
    private void Update() {
        if (!attached) {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser?.cameraRigController?.hud?.mainContainer != null)
            {
                questUI.transform.SetParent(localUser.cameraRigController.hud.mainContainer.transform);
                questUI.GetComponent<RectTransform>().localScale = new Vector3(hudScale, hudScale, hudScale);
                questUI.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
                questUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(Screen.width * Config.UI.questPositionX, Screen.height * Config.UI.questPositionY,0);

                attached = true;

            }
        }
    }
    public void UpdateData(Questing.PlayerData playerData) {


        if (playerData == null || playerData.complete) {
            Destroy(this);
        }
        else {
            Transform questCanvas = questUI.transform.Find("QuestCanvas");
            questCanvas.Find("RewardBorder").Find("RewardText").GetComponent<TextMeshProUGUI>().text = Language.GetString(PickupCatalog.GetPickupDef(playerData.reward).nameToken);
            questCanvas.Find("RewardBorder").Find("RewardText").GetComponent<TextMeshProUGUI>().color = PickupCatalog.GetPickupDef(playerData.reward).baseColor;
            questCanvas.Find("RewardBorder").Find("Reward").GetComponent<RawImage>().texture = PickupCatalog.GetPickupDef(playerData.reward).iconTexture;

            // Update QuestComponents
            int j = 0;
            for (int i = 0; i < playerData.questComponents.Count; i++) {
                if (!indexes.Contains(i) && !playerData.questComponents[i].complete) {
                    indexes.Add(i);
                    GameObject questComponent = Instantiate(assetBundle.LoadAsset<GameObject>("Assets/QuestComponent.prefab"));
                    questComponent.transform.Find("Description").GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
                    questComponent.transform.Find("Progress").GetComponent<TextMeshProUGUI>().font = RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont;
                    questComponent.transform.SetParent(questUI.transform);
                    questComponent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0);
                    questComponent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0);
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
                        questComponents[j].transform.Find("Description").GetComponent<TextMeshProUGUI>().text = questTypeDict[playerData.questComponents[i].questType];
                        questComponents[j].transform.Find("Progress").GetComponent<TextMeshProUGUI>().text = String.Format("{0}/{1}", playerData.questComponents[i].Progress, playerData.questComponents[i].objective);
                        float vel = progressVels[j];
                        questComponents[j].transform.Find("ProgressBorder").Find("Progress").transform.localScale = new Vector3(Mathf.SmoothDamp(questComponents[j].transform.Find("ProgressBorder").Find("Progress").transform.localScale.x, (float)playerData.questComponents[i].Progress/(float)playerData.questComponents[i].objective, ref vel, 0.7f),1,1);
                        progressVels[j] = vel;
                        questComponents[j].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 7.5f + (45 * j), 0);
                        j++;
                    }
                }
            }
            questUI.GetComponent<RectTransform>().sizeDelta = new Vector2(questUI.GetComponent<RectTransform>().sizeDelta.x, Mathf.SmoothDamp(questUI.GetComponent<RectTransform>().sizeDelta.y, origHeight - 15 + (45 * questComponents.Count), ref backgroundVel, 0.5f));
        }
    }
    private void OnDestroy()
    {
        Destroy(questUI);
    }
}
} // namespace RPGMod