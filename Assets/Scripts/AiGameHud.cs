using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AiGameHud : MonoBehaviour
{
    private TextMeshProUGUI status;
    private TMP_FontAsset runtimeFont;
    private GameObject modePanel;

    public void Initialize(GameManager manager, Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        runtimeFont = Resources.Load<TMP_FontAsset>("Fonts/GameFont");

        status = MakeText("Status", canvas.transform, "", 42);
        var sr = status.rectTransform;
        sr.anchorMin = sr.anchorMax = new Vector2(0, 1);
        sr.pivot = new Vector2(0, 1);
        sr.anchoredPosition = new Vector2(42, -34);
        sr.sizeDelta = new Vector2(1040, 92);
        status.alignment = TextAlignmentOptions.MidlineLeft;
        status.outlineWidth = 0.14f;
        status.outlineColor = new Color32(35, 22, 58, 255);
        var statusShadow = status.gameObject.AddComponent<Shadow>();
        statusShadow.effectColor = new Color(0.08f, 0.04f, 0.18f, 0.9f);
        statusShadow.effectDistance = new Vector2(3, -4);

        var restart = GameObject.Find("RestartButton");
        if (restart != null)
        {
            var spriteRenderer = restart.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var collider = restart.GetComponent<BoxCollider2D>();
                if (collider == null) collider = restart.AddComponent<BoxCollider2D>();
                collider.size = spriteRenderer.sprite.bounds.size;
                collider.offset = spriteRenderer.sprite.bounds.center;

                var restartInput = restart.GetComponent<RestartGameInput>();
                if (restartInput == null) restartInput = restart.AddComponent<RestartGameInput>();
                restartInput.Initialize(manager);
            }
        }
    }

    public void ShowModeSelection()
    {
        modePanel = new GameObject("ModeSelection", typeof(RectTransform), typeof(Image));
        modePanel.transform.SetParent(status.canvas.transform, false);
        modePanel.transform.SetAsLastSibling();
        var panelRect = modePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(960, 440);
        modePanel.GetComponent<Image>().color = new Color32(28, 25, 63, 244);
        modePanel.AddComponent<Outline>().effectColor = new Color32(255, 218, 120, 255);
        modePanel.GetComponent<Outline>().effectDistance = new Vector2(3, -3);

        var title = MakeText("ModeTitle", modePanel.transform, "Choose a mode", 58);
        title.alignment = TextAlignmentOptions.Center;
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.anchorMax = new Vector2(1, 1);
        title.rectTransform.pivot = new Vector2(0.5f, 1);
        title.rectTransform.anchoredPosition = new Vector2(0, -36);
        title.rectTransform.sizeDelta = new Vector2(-40, 88);
        title.outlineWidth = 0.14f;
        title.outlineColor = new Color32(18, 15, 42, 255);

        var hint = MakeText("ModeHint", modePanel.transform, "Choose how to play", 28);
        hint.alignment = TextAlignmentOptions.Center;
        hint.rectTransform.anchorMin = new Vector2(0, 1);
        hint.rectTransform.anchorMax = new Vector2(1, 1);
        hint.rectTransform.pivot = new Vector2(0.5f, 1);
        hint.rectTransform.anchoredPosition = new Vector2(0, -124);
        hint.rectTransform.sizeDelta = new Vector2(-40, 48);
        hint.color = new Color32(225, 220, 250, 255);
        hint.outlineWidth = 0.1f;
        hint.outlineColor = new Color32(18, 15, 42, 255);

        CreateModeButton("VsComputer", modePanel.transform, "Play vs. computer", "Single player", new Vector2(-218, -100), new Color32(101, 84, 225, 255), () => FindFirstObjectByType<GameManager>().StartLocalGame(true));
        CreateModeButton("LocalVersus", modePanel.transform, "Local 1v1", "Two players on one device", new Vector2(218, -100), new Color32(75, 82, 200, 255), () => FindFirstObjectByType<GameManager>().StartLocalGame(false));
    }

    public void HideModeSelection()
    {
        if (modePanel != null) modePanel.SetActive(false);
    }

    private void CreateModeButton(string name, Transform parent, string heading, string subtitle, Vector2 position, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(390, 178);
        var image = buttonObject.GetComponent<Image>();
        image.color = color;
        var outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color32(255, 218, 120, 255);
        outline.effectDistance = new Vector2(2, -2);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock { normalColor = Color.white, highlightedColor = new Color(1.1f, 1.08f, 1.18f), pressedColor = new Color(0.8f, 0.78f, 0.95f), selectedColor = Color.white, disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.8f), colorMultiplier = 1f, fadeDuration = 0.12f };
        button.onClick.AddListener(onClick);

        var label = MakeText("Label", buttonObject.transform, heading + "\n<size=24>" + subtitle + "</size>", 31);
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(18, 12);
        label.rectTransform.offsetMax = new Vector2(-18, -12);
        label.outlineWidth = 0.12f;
        label.outlineColor = new Color32(36, 28, 74, 255);
    }

    private TextMeshProUGUI MakeText(string name, Transform parent, string value, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = runtimeFont;
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    public void SetStatus(string value) { if (status != null) status.text = value; }
}

