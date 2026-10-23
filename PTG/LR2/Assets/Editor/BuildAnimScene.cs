using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildAnimScene
{
    const string SCENES = "Assets/Scenes";
    const string MATS = "Assets/Materials";
    const string ANIM = "Assets/Animator";
    const string CTRL = ANIM + "/Персонаж.controller";

    [MenuItem("Анимация/Собрать сцену и аниматор")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SCENES);
        Directory.CreateDirectory(MATS);
        Directory.CreateDirectory(ANIM);
        AssetDatabase.Refresh();

        EnsureLayer("Ground");
        var controller = BuildController();
        BuildScene(controller);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SCENES + "/Animation.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Сцена создана.");
    }

    static AnimatorController BuildController()
    {
        AssetDatabase.DeleteAsset(CTRL);
        var c=AnimatorController.CreateAnimatorControllerAtPath(CTRL);
        c.AddParameter("Speed",AnimatorControllerParameterType.Float);
        foreach(var name in new[]{"Run","Backward","Grounded","Falling"}) c.AddParameter(name,AnimatorControllerParameterType.Bool);
        c.AddParameter("Jump",AnimatorControllerParameterType.Trigger);
        c.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
        var sm=c.layers[0].stateMachine;
        var states=new System.Collections.Generic.Dictionary<string,AnimatorState>();
        string[] names={"Idle","Walk","BackWalk","Run","BackRun","Jump","Fall","Attack"};
        for(int i=0;i<names.Length;i++) {
            var state=sm.AddState(names[i],new Vector3(250+(i/4)*300,40+(i%4)*100,0));
            state.motion=CharacterClips.Create(names[i],ANIM); states[names[i]]=state;
        }
        sm.defaultState=states["Idle"]; states["Attack"].tag="Attack";
        foreach(var from in names) {
            if(from!="Fall") Transition(states[from],states["Fall"],false).AddCondition(AnimatorConditionMode.If,0,"Falling");
            if(from=="Attack") { var exit=Transition(states[from],states["Idle"],true); exit.exitTime=1; continue; }
            if(from=="Jump") continue;
            foreach(var to in new[]{"Idle","Walk","BackWalk","Run","BackRun"}) {
                if(from==to)continue;
                var tr=Transition(states[from],states[to],false);
                tr.AddCondition(AnimatorConditionMode.If,0,"Grounded");
                tr.AddCondition(to=="Idle"?AnimatorConditionMode.Less:AnimatorConditionMode.Greater,0.1f,"Speed");
                if(to!="Idle") {
                    tr.AddCondition(to.Contains("Run")?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Run");
                    tr.AddCondition(to.StartsWith("Back")?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Backward");
                }
            }
            foreach(var to in new[]{"Jump","Attack"}) {
                var tr=Transition(states[from],states[to],false); tr.AddCondition(AnimatorConditionMode.If,0,to); tr.AddCondition(AnimatorConditionMode.If,0,"Grounded");
            }
        }
        return c;
    }
    static AnimatorStateTransition Transition(AnimatorState a,AnimatorState b,bool exit) {
        var t=a.AddTransition(b); t.hasExitTime=exit; t.duration=0.08f; return t;
    }

    static void BuildScene(AnimatorController controller)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material floor = Mat("Пол", new Color(0.30f, 0.33f, 0.38f));
        Material block = Mat("Платформа", new Color(0.55f, 0.42f, 0.30f));
        Material body = Mat("Тело", new Color(0.22f, 0.56f, 0.82f));
        Material skin = Mat("Кожа", new Color(0.93f, 0.75f, 0.58f));

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.40f, 0.43f, 0.48f);
        RenderSettings.skybox = null;

        var sun = new GameObject("Солнце");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

        int ground = LayerMask.NameToLayer("Ground");
        var levelRoot = new GameObject("Уровень").transform;

        Box("Земля", new Vector3(0f, -0.5f, 0f), new Vector3(60f, 1f, 60f),
            floor, levelRoot, ground);

        float[,] blocks =
        {
            { 6f, 0.5f, 4f, 4f, 1f, 4f },
            { 11f, 1.2f, 7f, 4f, 2.4f, 4f },
            { -7f, 0.8f, 6f, 5f, 1.6f, 3f },
            { 0f, 1.6f, 14f, 8f, 3.2f, 4f },
            { -12f, 0.4f, -6f, 6f, 0.8f, 6f }
        };
        for (int i = 0; i < blocks.GetLength(0); i++)
            Box("Платформа_" + (i + 1),
                new Vector3(blocks[i, 0], blocks[i, 1], blocks[i, 2]),
                new Vector3(blocks[i, 3], blocks[i, 4] * 2f, blocks[i, 5]),
                block, levelRoot, ground);

        var player = new GameObject("Персонаж");
        player.transform.position = new Vector3(0f, 1.1f, -6f);

        CharacterClips.CreateModel(player.transform,body,skin);

        var capsule = player.AddComponent<CapsuleCollider>();
        capsule.height = 2f;
        capsule.radius = 0.45f;
        capsule.center = new Vector3(0f, 0f, 0f);

        var rb = player.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        var check = new GameObject("GroundCheck");
        check.transform.SetParent(player.transform, false);
        check.transform.localPosition = new Vector3(0f, -1.05f, 0f);

        var pc = player.AddComponent<PlayerController>();
        pc.groundCheck = check.transform;
        pc.groundMask = LayerMask.GetMask("Ground");

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.32f, 0.44f, 0.58f);
        cam.AddComponent<AudioListener>();
        var follow = cam.AddComponent<CameraFollow>();
        follow.target = player.transform;
        cam.transform.position = new Vector3(0f, 3.2f, -9.4f);

        EditorSceneManager.SaveScene(scene, SCENES + "/Animation.unity");
    }

    static GameObject Box(string name, Vector3 pos, Vector3 size, Material m,
                          Transform parent, int layer)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (layer >= 0) g.layer = layer;
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
