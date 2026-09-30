using SeaBattle.Core.Installers;
using SeaBattle.Core.UI.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Zenject;

public static class SampleSceneLayout
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const int Grid = 6;
    private const float Cell = 36f;

    [MenuItem("Sea Battle/Place Sample Scene UI")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (scene.isDirty)
                EditorSceneManager.SaveOpenScenes();

            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        RemoveNamed("Sea Battle Canvas");
        RemoveNamed("SceneContext");
        RemoveNamed("EventSystem");

        var paper = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Art/Resources/Sprites/cell_paper.png");
        var buttonSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvas = CreateCanvas();
        var background = CreatePanel(canvas.transform, "Background", new Color(0.04f, 0.06f, 0.1f), buttonSprite);
        Stretch(background);
        var layout = background.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 12;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var top = CreateRow(background, 40f);
        var topRow = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        topRow.spacing = 12f;
        topRow.childControlWidth = true;
        topRow.childControlHeight = true;
        topRow.childForceExpandWidth = true;
        topRow.childForceExpandHeight = true;
        var restart = CreateButton(top, "Перезапустить сцену", buttonSprite, font);
        var logToggle = CreateLogToggle(top, buttonSprite, font);

        var columns = CreateRow(background, 660f);
        var row = columns.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 16;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;
        var first = CreateColumn(columns, "Игрок 1", paper, buttonSprite, font);
        var second = CreateColumn(columns, "Игрок 2", paper, buttonSprite, font);

        var logHost = CreateRow(background, 220f);
        var log = CreateLog(logHost, font);

        var screen = canvas.gameObject.AddComponent<MatchScreenRefs>();
        screen.Assign(restart, logToggle, log, first, second);

        CreateContext();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static PlayerColumnRefs CreateColumn(RectTransform parent, string title, Sprite paper, Sprite buttonSprite, Font font)
    {
        var column = CreatePanel(parent, title, new Color(0.07f, 0.1f, 0.16f, 0.94f), buttonSprite);
        var element = column.gameObject.AddComponent<LayoutElement>();
        element.flexibleWidth = 1f;
        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateLabel(column, title, 22, 32, font);
        var status = CreateLabel(column, "Ожидание партии", 18, 36, font);
        CreateLabel(column, "Ваше поле", 16, 24, font);
        var own = CreateGrid(column, paper, font, false);
        CreateLabel(column, "Поле соперника", 16, 24, font);
        var enemy = CreateGrid(column, paper, font, true);

        var controls = CreateRow(column, 40f);
        var controlsRow = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
        controlsRow.spacing = 8;
        controlsRow.childAlignment = TextAnchor.MiddleCenter;
        controlsRow.childControlWidth = true;
        controlsRow.childControlHeight = true;
        controlsRow.childForceExpandWidth = true;
        controlsRow.childForceExpandHeight = true;
        var delay = CreateDelay(controls, buttonSprite, font);
        var disconnect = CreateButton(controls, "Разорвать", buttonSprite, font);
        var connect = CreateButton(controls, "Подключить", buttonSprite, font);

        var refs = column.gameObject.AddComponent<PlayerColumnRefs>();
        refs.Assign(status, own, enemy, disconnect, connect, delay);
        return refs;
    }

    private static CellView[] CreateGrid(RectTransform parent, Sprite paper, Font font, bool enemy)
    {
        var host = CreateRect(parent, enemy ? "EnemyGrid" : "OwnGrid");
        var element = host.gameObject.AddComponent<LayoutElement>();
        element.minHeight = Grid * Cell;
        element.preferredHeight = Grid * Cell;
        element.preferredWidth = Grid * Cell;
        var grid = host.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(Cell, Cell);
        grid.spacing = Vector2.zero;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Grid;
        grid.childAlignment = TextAnchor.UpperCenter;

        var cells = new CellView[Grid * Grid];
        var index = 0;
        for (var y = 0; y < Grid; y++)
        {
            for (var x = 0; x < Grid; x++)
            {
                cells[index] = CreateCell(host, x, y, paper, font);
                index++;
            }
        }

        return cells;
    }

    private static CellView CreateCell(RectTransform parent, int x, int y, Sprite paper, Font font)
    {
        var rect = CreateRect(parent, "Cell " + x + " " + y);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = paper;
        image.color = Color.white;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        var label = CreateText(rect, string.Empty, 16, TextAnchor.MiddleCenter, font);
        label.color = new Color(0.2f, 0.28f, 0.38f);
        label.raycastTarget = false;
        CreateOverlay(rect, "Ship");
        CreateOverlay(rect, "Mark");
        var cell = rect.gameObject.AddComponent<CellView>();
        cell.SetCoord(x, y);
        return cell;
    }

    private static void CreateOverlay(RectTransform parent, string name)
    {
        var rect = CreateRect(parent, name);
        Stretch(rect);
        var image = rect.gameObject.AddComponent<Image>();
        image.raycastTarget = false;
        image.enabled = false;
    }

    private static InputField CreateDelay(RectTransform parent, Sprite sprite, Font font)
    {
        var panel = CreatePanel(parent, "Delay", new Color(0.08f, 0.1f, 0.14f), sprite);
        var text = CreateText(panel, "400", 16, TextAnchor.MiddleCenter, font);
        var input = panel.gameObject.AddComponent<InputField>();
        input.textComponent = text;
        input.contentType = InputField.ContentType.IntegerNumber;
        input.text = "400";
        return input;
    }

    private static Toggle CreateLogToggle(RectTransform parent, Sprite sprite, Font font)
    {
        var panel = CreatePanel(parent, "LogToggle", new Color(0.2f, 0.32f, 0.48f), sprite);
        var toggle = panel.gameObject.AddComponent<Toggle>();
        var mark = CreatePanel(panel, "Mark", new Color(0.95f, 0.78f, 0.2f), sprite);
        mark.anchorMin = new Vector2(0f, 0.2f);
        mark.anchorMax = new Vector2(0f, 0.8f);
        mark.pivot = new Vector2(0f, 0.5f);
        mark.sizeDelta = new Vector2(18f, 0f);
        mark.anchoredPosition = new Vector2(10f, 0f);
        toggle.targetGraphic = panel.GetComponent<Image>();
        toggle.graphic = mark.GetComponent<Image>();
        toggle.isOn = false;
        var label = CreateText(panel, "Лог сообщений", 16, TextAnchor.MiddleCenter, font);
        label.rectTransform.offsetMin = new Vector2(36f, 0f);
        label.raycastTarget = false;
        return toggle;
    }

    private static Text CreateLog(RectTransform host, Font font)
    {
        var viewport = CreateRect(host, "Viewport");
        Stretch(viewport);
        viewport.offsetMax = new Vector2(-14f, 0f);
        var viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewport.gameObject.AddComponent<RectMask2D>();

        var textRect = CreateRect(viewport, "Text");
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(-12f, 0f);
        var text = textRect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 14;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        var fitter = textRect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollbar = CreateScrollbar(host);
        var scroll = host.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = textRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        return text;
    }

    private static Scrollbar CreateScrollbar(RectTransform parent)
    {
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var rect = CreateRect(parent, "Scrollbar");
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(12f, 0f);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.12f, 0.16f, 0.22f);
        var scrollbar = rect.gameObject.AddComponent<Scrollbar>();
        var handle = CreateRect(rect, "Handle");
        Stretch(handle);
        handle.offsetMin = new Vector2(2f, 2f);
        handle.offsetMax = new Vector2(-2f, -2f);
        var handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.sprite = sprite;
        handleImage.type = Image.Type.Sliced;
        handleImage.color = new Color(0.55f, 0.72f, 0.9f);
        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        return scrollbar;
    }

    private static Button CreateButton(RectTransform parent, string label, Sprite sprite, Font font)
    {
        var panel = CreatePanel(parent, label, new Color(0.2f, 0.32f, 0.48f), sprite);
        var button = panel.gameObject.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        var text = CreateText(panel, label, 16, TextAnchor.MiddleCenter, font);
        text.raycastTarget = false;
        return button;
    }

    private static Text CreateLabel(RectTransform parent, string content, int size, float height, Font font)
    {
        var host = CreateRow(parent, height);
        return CreateText(host, content, size, TextAnchor.MiddleCenter, font);
    }

    private static Text CreateText(RectTransform parent, string content, int size, TextAnchor anchor, Font font)
    {
        var rect = CreateRect(parent, "Text");
        Stretch(rect);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.text = content;
        return text;
    }

    private static RectTransform CreateRow(RectTransform parent, float height)
    {
        var row = CreateRect(parent, "Row");
        var element = row.gameObject.AddComponent<LayoutElement>();
        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleWidth = 1f;
        return row;
    }

    private static RectTransform CreatePanel(Transform parent, string name, Color color, Sprite sprite)
    {
        var rect = CreateRect(parent, name);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return rect;
    }

    private static RectTransform CreateRect(Transform parent, string name)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static RectTransform CreateCanvas()
    {
        var rect = new GameObject("Sea Battle Canvas", typeof(RectTransform)).GetComponent<RectTransform>();
        var canvas = rect.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = rect.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        rect.gameObject.AddComponent<GraphicRaycaster>();

        var system = new GameObject("EventSystem");
        system.AddComponent<EventSystem>();
        system.AddComponent<InputSystemUIInputModule>();
        return rect;
    }

    private static void CreateContext()
    {
        var root = new GameObject("SceneContext");
        var context = root.AddComponent<SceneContext>();
        var installer = root.AddComponent<SeaBattleSceneInstaller>();
        var serialized = new SerializedObject(context);
        var list = serialized.FindProperty("_monoInstallers");
        list.arraySize = 1;
        list.GetArrayElementAtIndex(0).objectReferenceValue = installer;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveNamed(string name)
    {
        var scene = EditorSceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == name)
                Object.DestroyImmediate(roots[i]);
        }
    }
}
