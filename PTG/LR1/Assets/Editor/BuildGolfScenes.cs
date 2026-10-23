using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildGolfScenes
{
    const string SCENES = "Assets/Scenes";
    const string MATS = "Assets/Materials";

    [MenuItem("Минигольф/Собрать обе сцены")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SCENES);
        Directory.CreateDirectory(MATS);
        EnsureTag("Ball");
        BuildLevel(1);
        BuildLevel(2);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SCENES + "/Level1.unity", true),
            new EditorBuildSettingsScene(SCENES + "/Level2.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Сцена создана.");
    }

    static void BuildLevel(int level)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material grass = Mat("Трава", new Color(0.24f, 0.55f, 0.24f));
        Material wall = Mat("Бортик", new Color(0.62f, 0.45f, 0.30f));
        Material ballMat = Mat("Шар", new Color(0.95f, 0.95f, 0.95f));
        Material accent = Mat("Акцент", new Color(0.95f, 0.42f, 0.14f));
        Material dark = Mat("Тёмный", new Color(0.12f, 0.12f, 0.14f));

        SetupEnvironment();

        var sun = new GameObject("Солнце");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(48f, 35f, 0f);

        var levelRoot = new GameObject("Уровень").transform;

        PhysicMaterial ground = GroundPhysics();

        float length = level == 1 ? 26f : 32f;
        var field = Box("Поле", new Vector3(0f, -0.25f, length / 2f - 3f),
                        new Vector3(7f, 0.5f, length), grass, levelRoot);
        field.GetComponent<Collider>().sharedMaterial = ground;

        float half = 3.5f;
        Wall("Бортик_Л", new Vector3(-half, 0.3f, length / 2f - 3f), new Vector3(0.4f, 1.1f, length),
             wall, levelRoot, ground);
        Wall("Бортик_П", new Vector3(half, 0.3f, length / 2f - 3f), new Vector3(0.4f, 1.1f, length),
             wall, levelRoot, ground);
        Wall("Бортик_Низ", new Vector3(0f, 0.3f, -3f), new Vector3(7f, 1.1f, 0.4f),
             wall, levelRoot, ground);
        Wall("Бортик_Верх", new Vector3(0f, 0.3f, length - 3f), new Vector3(7f, 1.1f, 0.4f),
             wall, levelRoot, ground);

        var manager = new GameObject("GameManager");
        var lm = manager.AddComponent<LevelManager>();

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Шар";
        ball.tag = "Ball";
        ball.transform.position = new Vector3(0f, 0.45f, -1.2f);
        ball.transform.localScale = Vector3.one * 0.5f;
        ball.GetComponent<Renderer>().sharedMaterial = ballMat;
        ball.GetComponent<Collider>().sharedMaterial = BallPhysics();
        var rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.45f;
        rb.drag = 0.4f;
        rb.angularDrag = 1.2f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.name = "Стрелка";
        Object.DestroyImmediate(arrow.GetComponent<Collider>());
        arrow.GetComponent<Renderer>().sharedMaterial = accent;
        arrow.transform.position = ball.transform.position;
        arrow.transform.localScale = new Vector3(0.12f, 0.02f, 1.6f);

        var bc = ball.AddComponent<BallController>();
        bc.arrow = arrow.transform;
        bc.level = lm;

        var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hole.name = "Лунка";
        hole.transform.position = new Vector3(0f, 0.02f, length - 5f);
        hole.transform.localScale = new Vector3(0.9f, 0.05f, 0.9f);
        hole.GetComponent<Renderer>().sharedMaterial = dark;
        var hc = hole.GetComponent<Collider>();
        hc.isTrigger = true;
        hole.AddComponent<Hole>().level = lm;
        hole.transform.SetParent(levelRoot);

        var fall = new GameObject("Зона_падения");
        var fbox = fall.AddComponent<BoxCollider>();
        fbox.isTrigger = true;
        fbox.size = new Vector3(80f, 2f, 80f);
        fall.transform.position = new Vector3(0f, -6f, length / 2f);
        fall.AddComponent<FallZone>().level = lm;

        if (level == 2)
        {
            var mill = new GameObject("Мельница");
            mill.transform.position = new Vector3(0f, 1.1f, 9f);
            mill.AddComponent<Windmill>();
            for (int i = 0; i < 4; i++)
            {
                var blade = Box("Лопасть_" + (i + 1), Vector3.zero,
                                new Vector3(0.25f, 3.2f, 0.5f), accent, mill.transform);
                blade.transform.localPosition = Vector3.zero;
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * 45f + 45f);
                blade.GetComponent<Collider>().sharedMaterial = GroundPhysics();
            }
            var post = Box("Столб", new Vector3(0f, 0.2f, 9f), new Vector3(0.3f, 1.8f, 0.3f),
                           wall, levelRoot);
            post.transform.position = new Vector3(0f, 0.2f, 9f);

            var arch = new GameObject("Дверь").transform;
            arch.SetParent(levelRoot);
            Box("Стойка_Л", new Vector3(-1.6f, 0.7f, 18f), new Vector3(0.5f, 2f, 0.5f), wall, arch);
            Box("Стойка_П", new Vector3(1.6f, 0.7f, 18f), new Vector3(0.5f, 2f, 0.5f), wall, arch);
            Box("Перекладина", new Vector3(0f, 1.9f, 18f), new Vector3(3.7f, 0.4f, 0.5f), wall, arch);
            var gate = Box("Заслонка", new Vector3(0f, 0.85f, 18f), new Vector3(2.8f, 1.7f, 0.3f),
                           accent, arch);
            gate.AddComponent<Door>();
        }

        BuildUI(lm);

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.42f, 0.62f, 0.82f);
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0f, 8f, -length / 2f + 1f);
        cam.transform.rotation = Quaternion.Euler(30f, 0f, 0f);

        EditorSceneManager.SaveScene(scene, SCENES + "/Level" + level + ".unity");
    }

    static void SetupEnvironment()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.45f, 0.50f);
        RenderSettings.skybox = null;
    }

    static void BuildUI(LevelManager lm)
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        lm.shotsText = MakeText(canvasGo.transform, "Ударов", "Ударов: 0",
                                new Vector2(0f, 1f), new Vector2(220f, -50f), 34, TextAnchor.UpperLeft);

        var barBack = MakeImage(canvasGo.transform, "Полоса_фон", new Vector2(0f, 0f),
                                new Vector2(230f, 60f), new Vector2(400f, 26f),
                                new Color(0f, 0f, 0f, 0.45f));
        var bar = MakeImage(barBack.transform, "Полоса", new Vector2(0.5f, 0.5f),
                            Vector2.zero, new Vector2(396f, 22f),
                            new Color(0.95f, 0.42f, 0.14f, 1f));
        bar.type = Image.Type.Filled;
        bar.fillMethod = Image.FillMethod.Horizontal;
        bar.fillAmount = 0f;
        lm.powerBar = bar;

        lm.winPanel = MakePanel(canvasGo.transform, "Панель_победа", "Уровень пройден!",
                                lm, true);
        lm.losePanel = MakePanel(canvasGo.transform, "Панель_поражение", "Шар улетел за поле",
                                 lm, false);
        lm.winPanel.SetActive(false);
        lm.losePanel.SetActive(false);
    }

    static GameObject MakePanel(Transform parent, string name, string caption,
                                LevelManager lm, bool win)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(760f, 380f);
        rt.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        MakeText(panel.transform, "Заголовок", caption, new Vector2(0.5f, 0.5f),
                 new Vector2(0f, 90f), 48, TextAnchor.MiddleCenter);

        MakeButton(panel.transform, "Кнопка_заново", "Заново", new Vector2(win ? -170f : 0f, -70f),
                   lm, lm.Restart);
        if (win)
            MakeButton(panel.transform, "Кнопка_дальше", "Следующий уровень",
                       new Vector2(170f, -70f), lm, lm.NextLevel);
        return panel;
    }

    static void MakeButton(Transform parent, string name, string caption, Vector2 pos,
                           LevelManager lm, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 80f);
        rt.anchoredPosition = pos;
        go.GetComponent<Image>().color = new Color(0.95f, 0.42f, 0.14f, 1f);
        MakeText(go.transform, "Текст", caption, new Vector2(0.5f, 0.5f), Vector2.zero,
                 30, TextAnchor.MiddleCenter);
        UnityEventTools.AddVoidPersistentListener(go.GetComponent<Button>().onClick, action);
    }

    static Text MakeText(Transform parent, string name, string value, Vector2 anchor,
                         Vector2 pos, int size, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700f, 90f);
        rt.anchoredPosition = pos;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = UIFont();
        text.fontSize = size;
        text.alignment = align;
        text.color = Color.white;
        return text;
    }

    static Image MakeImage(Transform parent, string name, Vector2 anchor, Vector2 pos,
                           Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static Font UIFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static GameObject Box(string name, Vector3 pos, Vector3 size, Material m, Transform parent)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (parent != null) g.transform.SetParent(parent, true);
        return g;
    }

    static void Wall(string name, Vector3 pos, Vector3 size, Material m, Transform parent,
                     PhysicMaterial phys)
    {
        var g = Box(name, pos, size, m, parent);
        g.GetComponent<Collider>().sharedMaterial = phys;
    }

    static PhysicMaterial GroundPhysics()
    {
        return PhysMat("Поле_физика", 0.35f, 0.3f);
    }

    static PhysicMaterial BallPhysics()
    {
        return PhysMat("Шар_физика", 0.25f, 0.45f);
    }

    static PhysicMaterial PhysMat(string name, float friction, float bounce)
    {
        string path = MATS + "/" + name + ".physicMaterial";
        var pm = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(path);
        if (pm == null)
        {
            pm = new PhysicMaterial(name);
            pm.dynamicFriction = friction;
            pm.staticFriction = friction;
            pm.bounciness = bounce;
            pm.bounceCombine = PhysicMaterialCombine.Maximum;
            AssetDatabase.CreateAsset(pm, path);
        }
        return pm;
    }

    static Material Mat(string name, Color color)
    {
        string path = MATS + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            m.color = color;
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static void EnsureTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var tags = so.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        so.ApplyModifiedProperties();
    }
}
