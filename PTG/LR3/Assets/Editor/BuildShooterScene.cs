using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class BuildShooterScene
{
    const string SCENES = "Assets/Scenes";
    const string MATS = "Assets/Materials";
    const string PREFABS = "Assets/Prefabs";

    [MenuItem("Тир/Собрать сцену")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SCENES);
        Directory.CreateDirectory(MATS);
        Directory.CreateDirectory(PREFABS);
        AssetDatabase.Refresh();
        EnsureTag("Player");

        var explosion = MakeParticlePrefab("Взрыв", new Color(1f, 0.55f, 0.15f), 26, 4.5f, 0.55f);
        var hit = MakeParticlePrefab("Попадание", new Color(1f, 0.9f, 0.6f), 10, 2.2f, 0.25f);
        var shell = MakeShellPrefab();
        var target = MakeTargetPrefab(explosion);

        BuildScene(shell, hit, target);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SCENES + "/Shooter.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Сцена создана.");
    }

    // ------------------------------------------------------------- префабы
    static GameObject MakeParticlePrefab(string name, Color color, int count,
                                         float speed, float life)
    {
        string path = PREFABS + "/" + name + ".prefab";
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = 0.16f;
        main.startColor = color;
        main.stopAction = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = ParticleMaterial();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject MakeShellPrefab()
    {
        string path = PREFABS + "/Гильза.prefab";
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Гильза";
        go.transform.localScale = new Vector3(0.035f, 0.06f, 0.035f);
        go.GetComponent<Renderer>().sharedMaterial = Mat("Латунь", new Color(0.85f, 0.68f, 0.25f));
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.02f;
        go.AddComponent<Shell>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject MakeTargetPrefab(GameObject explosion)
    {
        string path = PREFABS + "/Цель.prefab";
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Цель";
        go.GetComponent<Renderer>().sharedMaterial = Mat("Цель", new Color(0.90f, 0.24f, 0.22f));
        var t = go.AddComponent<Target>();
        t.explosion = explosion;
        t.health = 1;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ------------------------------------------------------------- сцена
    static void BuildScene(GameObject shell, GameObject hit, GameObject target)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material floorMat = Mat("Пол_тир", new Color(0.32f, 0.34f, 0.38f));
        Material wallMat = Mat("Стена", new Color(0.46f, 0.46f, 0.50f));
        Material gunMat = Mat("Оружие", new Color(0.16f, 0.17f, 0.20f));

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.40f, 0.42f, 0.46f);
        RenderSettings.skybox = null;

        var sun = new GameObject("Солнце");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(52f, 25f, 0f);

        var level = new GameObject("Уровень").transform;
        Box("Пол", new Vector3(0f, -0.5f, 0f), new Vector3(70f, 1f, 70f), floorMat, level);
        Box("Стена_С", new Vector3(0f, 2.5f, 35f), new Vector3(70f, 6f, 1f), wallMat, level);
        Box("Стена_Ю", new Vector3(0f, 2.5f, -35f), new Vector3(70f, 6f, 1f), wallMat, level);
        Box("Стена_З", new Vector3(-35f, 2.5f, 0f), new Vector3(1f, 6f, 70f), wallMat, level);
        Box("Стена_В", new Vector3(35f, 2.5f, 0f), new Vector3(1f, 6f, 70f), wallMat, level);
        Box("Укрытие_1", new Vector3(-8f, 1f, 10f), new Vector3(4f, 2f, 1.2f), wallMat, level);
        Box("Укрытие_2", new Vector3(9f, 1.5f, 18f), new Vector3(1.2f, 3f, 6f), wallMat, level);
        Box("Укрытие_3", new Vector3(2f, 0.8f, -12f), new Vector3(7f, 1.6f, 1.2f), wallMat, level);

        // ---- игрок
        var player = new GameObject("Игрок");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.1f, -22f);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.9f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0f, 0f);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.30f, 0.38f, 0.48f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.SetParent(player.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        var fps = player.AddComponent<FirstPersonController>();
        fps.cam = camGo.transform;

        // ---- оружие
        var gun = Box("Оружие", Vector3.zero, new Vector3(0.14f, 0.14f, 0.75f), gunMat, null);
        Object.DestroyImmediate(gun.GetComponent<Collider>());
        gun.transform.SetParent(camGo.transform, false);
        gun.transform.localPosition = new Vector3(0.28f, -0.24f, 0.55f);

        var grip = Box("Рукоять", Vector3.zero, new Vector3(0.12f, 0.3f, 0.14f), gunMat, null);
        Object.DestroyImmediate(grip.GetComponent<Collider>());
        grip.transform.SetParent(gun.transform, false);
        grip.transform.localPosition = new Vector3(0f, -1.4f, -0.35f);
        grip.transform.localScale = new Vector3(0.9f, 2.1f, 0.19f);

        var muzzle = new GameObject("MuzzlePoint");
        muzzle.transform.SetParent(gun.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, 0f, 0.55f);
        var flash = muzzle.AddComponent<ParticleSystem>();
        var fmain = flash.main;
        fmain.duration = 0.2f;
        fmain.loop = false;
        fmain.playOnAwake = false;
        fmain.startLifetime = 0.06f;
        fmain.startSpeed = 3.5f;
        fmain.startSize = 0.12f;
        fmain.startColor = new Color(1f, 0.85f, 0.4f);
        var femission = flash.emission;
        femission.rateOverTime = 0f;
        femission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });
        var fshape = flash.shape;
        fshape.shapeType = ParticleSystemShapeType.Cone;
        fshape.angle = 14f;
        fshape.radius = 0.02f;
        muzzle.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();

        var shellPoint = new GameObject("ShellPoint");
        shellPoint.transform.SetParent(gun.transform, false);
        shellPoint.transform.localPosition = new Vector3(0.7f, 0.3f, -0.1f);

        var weapon = gun.AddComponent<Weapon>();
        weapon.cam = cam;
        weapon.shellPoint = shellPoint.transform;
        weapon.shellPrefab = shell;
        weapon.hitEffect = hit;
        weapon.muzzleFlash = flash;

        // ---- зоны появления целей
        var zones = new GameObject("Зоны").transform;
        Vector3[] zonePos =
        {
            new Vector3(0f, 0f, -6f),
            new Vector3(-12f, 0f, 8f),
            new Vector3(12f, 0f, 16f)
        };
        for (int i = 0; i < zonePos.Length; i++)
        {
            var z = new GameObject("Зона_" + (i + 1));
            z.transform.SetParent(zones, false);
            z.transform.position = zonePos[i];
            var box = z.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, 6f);
            box.center = new Vector3(0f, 1.5f, 0f);
            var tz = z.AddComponent<TargetZone>();
            tz.targetPrefab = target;
            tz.count = 4;
            tz.area = new Vector3(14f, 3.5f, 14f);
        }

        // ---- интерфейс
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

        Crosshair(canvasGo.transform, new Vector2(26f, 3f));
        Crosshair(canvasGo.transform, new Vector2(3f, 26f));

        var scoreText = MakeText(canvasGo.transform, "Очки", "Очки: 0",
                                 new Vector2(0f, 1f), new Vector2(210f, -50f), 36,
                                 TextAnchor.UpperLeft);
        var manager = new GameObject("GameManager");
        manager.AddComponent<ScoreManager>().scoreText = scoreText;

        EditorSceneManager.SaveScene(scene, SCENES + "/Shooter.unity");
    }

    static void Crosshair(Transform parent, Vector2 size)
    {
        var go = new GameObject("Прицел", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);
        go.GetComponent<Image>().raycastTarget = false;
    }

    static Text MakeText(Transform parent, string name, string value, Vector2 anchor,
                         Vector2 pos, int size, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(400f, 80f);
        rt.anchoredPosition = pos;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = UIFont();
        text.fontSize = size;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
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

    static Material ParticleMaterial()
    {
        string path = MATS + "/Частицы.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader sh = Shader.Find("Particles/Standard Unlit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
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
