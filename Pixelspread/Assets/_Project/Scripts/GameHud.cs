using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Code-built HUD using uGUI (Canvas). World adds this automatically, no scene setup needed.
public class GameHud : MonoBehaviour
{
    static readonly Color PanelColor = new Color(0.96f, 0.96f, 0.97f, 0.96f);
    static readonly Color ButtonColor = new Color(0.88f, 0.89f, 0.92f, 1f);
    static readonly Color ButtonHoverColor = new Color(0.80f, 0.83f, 0.90f, 1f);
    static readonly Color ButtonPressedColor = new Color(0.72f, 0.76f, 0.86f, 1f);
    static readonly Color ButtonDisabledColor = new Color(0.90f, 0.90f, 0.92f, 0.6f);
    static readonly Color TextColor = new Color(0.15f, 0.16f, 0.20f, 1f);
    static readonly Color TextDisabledColor = new Color(0.15f, 0.16f, 0.20f, 0.4f);

    static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    World world;
    Font font;
    bool built;

    GameObject upgradePanel;
    Text coinText, toggleLabel, titleText, brushText, nextText, buyLabel;
    Button buyButton;

    long lastCoins = -1;
    int lastLevel = -1;
    bool panelOpen;

    public void Init(World w)
    {
        if (built) return;
        built = true;

        world = w;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        EnsureEventSystem();
        BuildUi();
        Refresh();
    }

    // True if the given screen position (Input System coordinates) is over any UI element.
    public bool IsPointerOver(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var data = new PointerEventData(EventSystem.current) { position = screenPos };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(data, raycastResults);
        return raycastResults.Count > 0;
    }

    void Update()
    {
        if (!built) return;

        long coins = world.Economy.Coins;
        int level = world.Upgrades.TapLevel;
        if (coins == lastCoins && level == lastLevel) return;

        Refresh();
    }

    void Refresh()
    {
        Upgrades up = world.Upgrades;
        long coins = world.Economy.Coins;
        lastCoins = coins;
        lastLevel = up.TapLevel;

        coinText.text = "Coins  " + Economy.Format(coins);
        titleText.text = "Tap power   Lv " + up.TapLevel + " / " + Upgrades.MaxTapLevel;
        brushText.text = "Brush: " + up.CurrentBrushName;

        if (up.CanUpgradeTap)
        {
            nextText.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(true);
            nextText.text = "Next: " + up.NextBrushName;
            buyLabel.text = "Buy  -  " + Economy.Format(up.NextTapCost);

            bool canAfford = coins >= up.NextTapCost;
            buyButton.interactable = canAfford;
            buyLabel.color = canAfford ? TextColor : TextDisabledColor;
        }
        else
        {
            nextText.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
            brushText.text = "Brush: " + up.CurrentBrushName + "   (max level)";
        }
    }

    void TogglePanel()
    {
        panelOpen = !panelOpen;
        upgradePanel.SetActive(panelOpen);
        toggleLabel.text = panelOpen ? "Close" : "Upgrades";
    }

    void BuyTapPower()
    {
        world.Upgrades.TryBuyTapPower();
        Refresh();
    }

    // ---------- Setup ----------

    void EnsureEventSystem()
    {
        var es = FindAnyObjectByType<EventSystem>();
        if (es == null)
            es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        // The old Input Manager module errors when the project uses the new Input System.
        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null) Destroy(old);

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    void BuildUi()
    {
        var canvasGo = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Transform root = canvasGo.transform;

        // Coin counter, top centre
        RectTransform coinRect = NewRect("Coin Panel", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(380f, 72f));
        AddImage(coinRect, PanelColor);
        coinText = NewText("Coin Text", coinRect, 36, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(coinText.rectTransform);

        // Upgrades button, bottom centre
        Button toggle = NewButton("Upgrades Button", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 20f), new Vector2(300f, 76f), out toggleLabel);
        toggleLabel.text = "Upgrades";
        toggle.onClick.AddListener(TogglePanel);

        // Upgrade panel, above the button
        RectTransform panelRect = NewRect("Upgrade Panel", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 116f), new Vector2(720f, 300f));
        AddImage(panelRect, PanelColor);
        upgradePanel = panelRect.gameObject;

        titleText = NewText("Title", panelRect, 34, TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(titleText.rectTransform, new Vector2(32f, -24f), new Vector2(656f, 50f));

        brushText = NewText("Brush", panelRect, 30, TextAnchor.MiddleLeft, FontStyle.Normal);
        Place(brushText.rectTransform, new Vector2(32f, -84f), new Vector2(656f, 44f));

        nextText = NewText("Next", panelRect, 30, TextAnchor.MiddleLeft, FontStyle.Normal);
        Place(nextText.rectTransform, new Vector2(32f, -132f), new Vector2(656f, 44f));

        buyButton = NewButton("Buy Button", panelRect, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(32f, -196f), new Vector2(656f, 76f), out buyLabel);
        buyButton.onClick.AddListener(BuyTapPower);

        upgradePanel.SetActive(false);
    }

    // ---------- Small UI helpers ----------

    static RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image AddImage(RectTransform rt, Color color)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Top-left anchored placement inside a panel.
    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    Text NewText(string name, Transform parent, int size, TextAnchor align, FontStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = TextColor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button NewButton(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, out Text label)
    {
        RectTransform rt = NewRect(name, parent, anchor, pivot, pos, size);
        Image img = AddImage(rt, Color.white);

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHoverColor;
        colors.selectedColor = ButtonColor;
        colors.pressedColor = ButtonPressedColor;
        colors.disabledColor = ButtonDisabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.05f;
        btn.colors = colors;

        label = NewText("Label", rt, 32, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(label.rectTransform);
        return btn;
    }
}