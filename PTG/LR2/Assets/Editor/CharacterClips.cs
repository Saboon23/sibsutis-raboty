using UnityEngine;
using UnityEditor;

public static class CharacterClips
{
    public static void CreateModel(Transform root, Material body, Material skin)
    {
        var visual=new GameObject("Model").transform; visual.SetParent(root,false);
        Part("Torso",visual,new Vector3(0,0.2f,0),new Vector3(.65f,.8f,.38f),body);
        Part("Head",visual,new Vector3(0,.88f,0),Vector3.one*.45f,skin);
        Part("Nose",visual,new Vector3(0,.88f,.25f),new Vector3(.12f,.12f,.16f),skin);
        foreach(int side in new[]{-1,1}) {
            string suffix=side<0?"L":"R";
            var arm=Joint("Arm"+suffix,visual,new Vector3(side*.43f,.53f,0));
            Part("Upper",arm,new Vector3(0,-.22f,0),new Vector3(.22f,.46f,.24f),body);
            var elbow=Joint("Elbow",arm,new Vector3(0,-.43f,0));
            Part("Lower",elbow,new Vector3(0,-.2f,0),new Vector3(.18f,.4f,.2f),skin);
            var leg=Joint("Leg"+suffix,visual,new Vector3(side*.2f,-.18f,0));
            Part("Thigh",leg,new Vector3(0,-.22f,0),new Vector3(.25f,.44f,.29f),body);
            var knee=Joint("Knee",leg,new Vector3(0,-.44f,0));
            Part("Shin",knee,new Vector3(0,-.19f,0),new Vector3(.21f,.38f,.23f),body);
            Part("Foot",knee,new Vector3(0,-.36f,.10f),new Vector3(.26f,.16f,.43f),skin);
        }
    }
    static Transform Joint(string name,Transform parent,Vector3 pos) {
        var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=pos; return t;
    }
    static void Part(string name,Transform parent,Vector3 pos,Vector3 size,Material mat) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;
        Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(parent,false);
        g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;
    }
    public static AnimationClip Create(string name,string folder)
    {
        var c=new AnimationClip{name=name,frameRate=30};
        bool locomotion=name.Contains("Walk")||name.Contains("Run");
        bool run=name.Contains("Run"), backward=name.StartsWith("Back");
        float duration=name=="Attack"?.8f:name=="Jump"?.35f:run?.55f:1f;
        string[] bones={"ArmL","ArmR","ArmL/Elbow","ArmR/Elbow","LegL","LegR","LegL/Knee","LegR/Knee","Torso","Head"};
        foreach(string bone in bones) {
            var curve=new AnimationCurve();
            for(int i=0;i<=30;i++) {
                float u=i/30f, s=Mathf.Sin(u*2*Mathf.PI), angle=0;
                bool right=bone.Contains("R");float phase=right?-1:1;
                if(locomotion) {
                    float stride=(run?48:27)*(backward?-1:1)*s*phase;
                    if(bone.StartsWith("Leg")) angle=bone.Contains("Knee")?Mathf.Max(0,-stride)*1.2f:stride;
                    if(bone.StartsWith("Arm")) angle=bone.Contains("Elbow")?(run?-65:-15):-stride*.8f;
                    if(bone=="Torso")angle=run?10:0;
                } else if(name=="Idle") {
                    if(bone.StartsWith("Arm"))angle=bone.Contains("Elbow")?-8:3*s;
                } else if(name=="Jump") {
                    if(bone.StartsWith("Arm"))angle=-70*Mathf.Sin(u*Mathf.PI);
                    if(bone.StartsWith("Leg"))angle=bone.Contains("Knee")?30*Mathf.Sin(u*Mathf.PI):-18*Mathf.Sin(u*Mathf.PI);
                } else if(name=="Fall") {
                    if(bone.StartsWith("Arm"))angle=bone.Contains("Elbow")?-45:-50;
                    if(bone.StartsWith("Leg"))angle=bone.Contains("Knee")?25:-15;
                } else if(name=="Attack") {
                    float strike=Mathf.Sin(Mathf.Clamp01(u/.7f)*Mathf.PI);
                    if(bone=="ArmR")angle=-110*strike;
                    if(bone=="ArmR/Elbow")angle=-25*(1-strike);
                    if(bone=="Torso")angle=12*strike;
                }
                curve.AddKey(u*duration,angle);
            }
            c.SetCurve("Model/"+bone,typeof(Transform),"localEulerAnglesRaw.x",curve);
        }
        var settings=AnimationUtility.GetAnimationClipSettings(c);
        settings.loopTime=name!="Jump"&&name!="Attack";AnimationUtility.SetAnimationClipSettings(c,settings);
        string path=folder+"/"+name+".anim";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(c,path);return c;
    }
}
