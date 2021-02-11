using UnityEngine;
using UnityEngine.UI;

namespace RPGMod
{
public static class UI {
    public static AssetBundle assetBundle;
    public static float hudScale { get; private set; }
    public static void Setup() {
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
    public static void AddBorder(GameObject element) {
        GameObject border = new GameObject();

        border.name = "border";

        border.AddComponent<RectTransform>();
        border.AddComponent<Image>();

        border.transform.SetParent(element.transform);

        border.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        border.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        border.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        border.GetComponent<RectTransform>().sizeDelta = new Vector2(element.GetComponent<RectTransform>().sizeDelta.x + 2, element.GetComponent<RectTransform>().sizeDelta.y + 2);
        border.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);

        border.GetComponent<Image>().sprite = UnityEngine.Object.Instantiate(assetBundle.LoadAsset<Sprite>("Assets/UIBorder.png"));;
        border.GetComponent<Image>().type = Image.Type.Sliced;
    }
}
} // namespace RPGMod