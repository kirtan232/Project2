using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the whole HUD with Unity UI (uGUI) at startup: lives, weapons, bullets, kills,
/// treasures, crosshair and the win / game-over panel.
/// </summary>
public class GameHUD : MonoBehaviour
{
    PlayerHealth health;
    PlayerWeapons weapons;
    Text statsText, weaponText, messageText;
    GameObject endPanel;
    Font font;

    void Start()
    {
        health = FindFirstObjectByType<PlayerHealth>();
        weapons = FindFirstObjectByType<PlayerWeapons>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        BuildUI();

        if (health != null) health.LivesChanged += _ => Refresh();
        if (weapons != null) weapons.Changed += Refresh;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StatsChanged += Refresh;
            GameManager.Instance.GameEnded += ShowEnd;
        }
        Refresh();
    }

    void BuildUI()
    {
        var canvasGo = new GameObject("HUD Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        statsText = MakeText(canvasGo.transform, "Stats", TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(30, -30), new Vector2(700, 260), 36);
        weaponText = MakeText(canvasGo.transform, "Weapons", TextAnchor.LowerLeft,
            new Vector2(0, 0), new Vector2(30, 30), new Vector2(900, 160), 34);

        var cross = MakeText(canvasGo.transform, "Crosshair", TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60), 48);
        cross.text = "+";

        // End-of-game panel
        endPanel = new GameObject("End Panel");
        endPanel.transform.SetParent(canvasGo.transform, false);
        var img = endPanel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        messageText = MakeText(endPanel.transform, "Message", TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 400), 70);
        endPanel.SetActive(false);
    }

    Text MakeText(Transform parent, string name, TextAnchor anchor, Vector2 anchorPoint,
                  Vector2 pos, Vector2 size, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchorPoint;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return t;
    }

    void Refresh()
    {
        var gm = GameManager.Instance;
        if (statsText != null)
        {
            int lives = health != null ? health.Lives : 0;
            statsText.text = "Lives: " + lives
                + "\nKills: " + (gm != null ? gm.Kills : 0)
                + "\nTreasures: " + (gm != null ? gm.TreasuresCollected : 0) + "/" + (gm != null ? gm.totalTreasures : 3)
                + "\nGuards left: " + (gm != null ? gm.GuardsAlive : 0);
        }

        if (weaponText != null && weapons != null)
        {
            string s = "Weapons: ";
            if (!weapons.HasGun && weapons.Grenades == 0) s += "none";
            if (weapons.HasGun)
                s += (weapons.Current == WeaponType.Gun ? "[Gun]" : "Gun") + "  ";
            if (weapons.Grenades > 0)
                s += (weapons.Current == WeaponType.Grenade ? "[Grenades]" : "Grenades") + " x" + weapons.Grenades;
            s += "\nBullets: " + (weapons.HasGun ? weapons.Ammo.ToString() : "-");
            weaponText.text = s + "\n(Q/E switch)";
        }
    }

    void ShowEnd(bool won)
    {
        endPanel.SetActive(true);
        messageText.text = won
            ? "YOU WIN!\nTreasure taken, guards eliminated.\n\nPress R to restart"
            : "GAME OVER\n\nPress R to restart";
    }
}
