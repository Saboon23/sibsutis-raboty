using System.IO;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BuildMonsterWave
{
    const string SCENES = "Assets/Scenes";
    const string MATS = "Assets/Materials";
    const string PREFABS = "Assets/Prefabs";
    const string ANIM = "Assets/Monster";

    [MenuItem("Волна монстров/Собрать сцену")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SCENES);
        Directory.CreateDirectory(MATS);
        Directory.CreateDirectory(PREFABS);
        Directory.CreateDirectory(ANIM);
        AssetDatabase.Refresh();
        EnsureLayer("Monsters");

        var controller = BuildController();
        BuildScene(controller);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SCENES + "/MonsterWave.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Сцена создана.");
    }

    // ------------------------------------------------------------- аниматор
    static AnimatorController BuildController()
    {
        string path = ANIM + "/Монстр.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);
        var c = AnimatorController.CreateAnimatorControllerAtPath(path);
        c.AddParameter("Attack", AnimatorControllerParameterType.Bool);
        c.AddParameter("Die", AnimatorControllerParameterType.Bool);

        var runClip = MakeClip("Run", 0.8f, 0.06f, true, false);
        var attackClip = MakeClip("Attack", 0.6f, 0.16f, true, false);
        var dieClip = MakeClip("Die", 1.2f, -0.9f, false, true);

        var sm = c.layers[0].stateMachine;
        var run = sm.AddState("Run", new Vector3(260f, 60f, 0f));
        var attack = sm.AddState("Attack", new Vector3(260f, 170f, 0f));
        var die = sm.AddState("Die", new Vector3(260f, 280f, 0f));
        run.motion = runClip;
        attack.motion = attackClip;
        die.motion = dieClip;
        sm.defaultState = run;

        var ra = run.AddTransition(attack);
        ra.hasExitTime = false;
        ra.duration = 0.1f;
        ra.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        var ad = attack.AddTransition(die);
        ad.hasExitTime = false;
        ad.duration = 0.05f;
        ad.AddCondition(AnimatorConditionMode.If, 0f, "Die");

        var rd = run.AddTransition(die);
        rd.hasExitTime = false;
        rd.duration = 0.05f;
        rd.AddCondition(AnimatorConditionMode.If, 0f, "Die");

        AssetDatabase.SaveAssets();
        return c;
    }

    static AnimationClip MakeClip(string name, float length, float amplitude,
                                  bool loop, bool addDieEvent)
    {
        string path = ANIM + "/" + name + ".anim";
        var clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = 30f;

        AnimationCurve curve;
        if (loop)
            curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(length * 0.5f, amplitude),
                new Keyframe(length, 0f));
        else
            curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(length * 0.75f, amplitude),
                new Keyframe(length, amplitude));
        clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", curve);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (addDieEvent)
        {
            var ev = new AnimationEvent();
            ev.time = length;
            ev.functionName = "Die";
            AnimationUtility.SetAnimationEvents(clip, new[] { ev });
        }

        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    // ------------------------------------------------------------- сцена
    static void BuildScene(AnimatorController controller)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.45f, 0.48f);
        RenderSettings.skybox = null;

        Material grass = Mat("Земля", new Color(0.28f, 0.45f, 0.24f));
        Material stone = Mat("Камень", new Color(0.52f, 0.52f, 0.55f));
        Material wood = Mat("Дерево", new Color(0.42f, 0.30f, 0.18f));
        Material leaf = Mat("Крона", new Color(0.20f, 0.48f, 0.22f));
        Material skin = Mat("Монстр", new Color(0.62f, 0.25f, 0.55f));

        var sun = new GameObject("Солнце");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, 40f, 0f);

        var level = new GameObject("Level").transform;
        Box("Земля", new Vector3(0f, -0.5f, 0f), new Vector3(60f, 1f, 60f), grass, level);

        var rnd = new System.Random(7);
        for (int i = 0; i < 18; i++)
        {
            float x = (float)(rnd.NextDouble() * 48.0 - 24.0);
            float z = (float)(rnd.NextDouble() * 48.0 - 24.0);
            if (Mathf.Abs(x) < 5f && Mathf.Abs(z) < 5f) continue;
            if (i % 3 == 0)
            {
                Box("Камень_" + i, new Vector3(x, 0.5f, z),
                    new Vector3(1.4f, 1f, 1.4f), stone, level);
            }
            else
            {
                var trunk = Cyl("Ствол_" + i, new Vector3(x, 1.2f, z),
                                new Vector3(0.4f, 1.2f, 0.4f), wood, level);
                var crown = Sphere("Крона_" + i, new Vector3(x, 3.1f, z),
                                   Vector3.one * 2.4f, leaf, level);
                crown.transform.SetParent(trunk.transform, true);
            }
        }

        var well = Cyl("Колодец", new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 0.6f, 2.4f),
                       stone, level);

        foreach (var t in level.GetComponentsInChildren<Transform>())
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.NavigationStatic);
        NavMeshBuilder.BuildNavMesh();

        // ---- монстр
        var monster = Caps("Монстр", Vector3.zero, skin);
        monster.layer = LayerMask.NameToLayer("Monsters");
        var eye = Sphere("Глаз", new Vector3(0f, 0.35f, 0.42f), Vector3.one * 0.25f,
                         Mat("Глаз", new Color(0.95f, 0.92f, 0.2f)), null);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        eye.transform.SetParent(monster.transform, false);
        eye.transform.localPosition = new Vector3(0f, 0.35f, 0.42f);
        eye.transform.localScale = Vector3.one * 0.25f;
        eye.layer = monster.layer;

        Object.DestroyImmediate(monster.GetComponent<Collider>());
        var col = monster.AddComponent<BoxCollider>();
        col.size = new Vector3(1.1f, 2f, 1.1f);

        var agent = monster.AddComponent<NavMeshAgent>();
        agent.speed = 3f;
        agent.angularSpeed = 240f;
        agent.acceleration = 8f;

        var animator = monster.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        var ms = monster.AddComponent<MonsterScript>();
        ms.attack_distance = 5f;
        ms.target = well.transform;

        string monsterPath = PREFABS + "/Монстр.prefab";
        var monsterPrefab = PrefabUtility.SaveAsPrefabAsset(monster, monsterPath);
        Object.DestroyImmediate(monster);

        // ---- генераторы
        var spawners = new GameObject("Генераторы").transform;
        Vector3[] pos =
        {
            new Vector3(-22f, 1.1f, -22f), new Vector3(22f, 1.1f, -22f),
            new Vector3(-22f, 1.1f, 22f), new Vector3(22f, 1.1f, 22f)
        };
        for (int i = 0; i < pos.Length; i++)
        {
            var sp = new GameObject("Spawner_" + (i + 1));
            sp.transform.SetParent(spawners, false);
            sp.transform.position = pos[i];
            var s = sp.AddComponent<SpawnerScript>();
            s.target = well.transform;
            s.monster = monsterPrefab;
            s.spawnTime = 2f;
            s.minimalSpawnTime = 1f;
        }

        // ---- камера и интерфейс
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.40f, 0.58f, 0.78f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 24f, -20f);
        camGo.transform.rotation = Quaternion.Euler(52f, 0f, 0f);

        var mouse = camGo.AddComponent<MouseScript>();
        mouse.cam = cam;
        mouse.monsters = LayerMask.GetMask("Monsters");

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

        var textGo = new GameObject("Очки", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(canvasGo.transform, false);
        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(400f, 80f);
        rt.anchoredPosition = new Vector2(210f, -50f);
        var text = textGo.GetComponent<Text>();
        text.text = "Очки: 0";
        text.fontSize = 36;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var gm = new GameObject("GameManager");
        var sc = gm.AddComponent<ScoreManager>();
        sc.scoreText = text;
        sc.pointsPerMonster = 10;

        EditorSceneManager.SaveScene(scene, SCENES + "/MonsterWave.unity");
    }

    // ------------------------------------------------------------- помощники
    static GameObject Box(string name, Vector3 pos, Vector3 size, Material m, Transform parent)
    {
        return Prim(PrimitiveType.Cube, name, pos, size, m, parent);
    }

    static GameObject Cyl(string name, Vector3 pos, Vector3 size, Material m, Transform parent)
    {
        return Prim(PrimitiveType.Cylinder, name, pos, size, m, parent);
    }

    static GameObject Sphere(string name, Vector3 pos, Vector3 size, Material m, Transform parent)
    {
        return Prim(PrimitiveType.Sphere, name, pos, size, m, parent);
    }

    static GameObject Caps(string name, Vector3 pos, Material m)
    {
        return Prim(PrimitiveType.Capsule, name, pos, Vector3.one, m, null);
    }

    static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 size,
                           Material m, Transform parent)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (parent != null) g.transform.SetParent(parent, true);
        return g;
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

    static void EnsureLayer(string layer)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var layers = so.FindProperty("layers");
        for (int i = 0; i < layers.arraySize; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == layer) return;
        for (int i = 8; i < layers.arraySize; i++)
        {
            var el = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(el.stringValue))
            {
                el.stringValue = layer;
                so.ApplyModifiedProperties();
                return;
            }
        }
    }
}
