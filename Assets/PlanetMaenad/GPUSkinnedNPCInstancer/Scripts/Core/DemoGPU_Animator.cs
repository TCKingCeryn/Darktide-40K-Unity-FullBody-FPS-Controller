using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DemoGPU_Animator : MonoBehaviour
{
    [Space(10)]
    public Renderer MeshRenderer;
    [SerializeField]
    List<AnimationFrameInfo> FrameInformations;
    [Space(5)]


    public bool UseSingleStartingAnimation;
    public string SingleStartingAnimation;
    [Space(5)]





    public string ManualAnimationName;
    [Space(5)]
    public string CurrentPlayingAnimation;
    public float CurrentPlayingOffset;


    public bool IsPlaying { get; set; }



    internal Renderer Renderer
    {
        get { return MeshRenderer ?? (MeshRenderer = GetComponent<Renderer>()); }
    }
    internal MaterialPropertyBlock _materialPropertyBlock;
    internal MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            return _materialPropertyBlock ?? (_materialPropertyBlock = new MaterialPropertyBlock());
        }
    }

    internal AnimationFrameInfo CurrentAnimationFrameInfo;


    void OnEnable()
    {
        if (UseSingleStartingAnimation)
        {
            var offsetSeconds = Random.Range(0.0f, 3.0f);

            Play(SingleStartingAnimation, offsetSeconds);
        }     
    }


    public void Play(string animationName, float offsetSeconds)
    {
        //Search for Animation Name
        CurrentAnimationFrameInfo = FrameInformations.First(x => x.Name == animationName);

        //Start Frame
        SetMaterialProperty("_OffsetSeconds", offsetSeconds);

        //Set Frame Values
        SetMaterialProperty("_StartFrame", CurrentAnimationFrameInfo.StartFrame);
        SetMaterialProperty("_EndFrame", CurrentAnimationFrameInfo.EndFrame);
        SetMaterialProperty("_FrameCount", CurrentAnimationFrameInfo.FrameCount);

        //Set Animation Values
        ApplyMaterial();

        IsPlaying = true;

        CurrentPlayingAnimation = animationName;
        CurrentPlayingOffset = offsetSeconds;
    }
    public void PlayOnce(string animationName, float offsetSeconds)
    {
        StartCoroutine(PlayOnceDelay(animationName, offsetSeconds));
    }
    IEnumerator PlayOnceDelay(string animationName, float offsetSeconds)
    {
        //Search for Animation Name
        CurrentAnimationFrameInfo = FrameInformations.First(x => x.Name == animationName);

        //Start / Stop Frame
        SetMaterialProperty("_OffsetSeconds", offsetSeconds);

        //Set Frame Values
        SetMaterialProperty("_StartFrame", CurrentAnimationFrameInfo.StartFrame);
        SetMaterialProperty("_EndFrame", CurrentAnimationFrameInfo.EndFrame);
        SetMaterialProperty("_FrameCount", CurrentAnimationFrameInfo.FrameCount);

        //Set Animation Values
        ApplyMaterial();

        IsPlaying = true;

        CurrentPlayingAnimation = animationName;
        CurrentPlayingOffset = offsetSeconds;

        yield return new WaitForSeconds(CurrentAnimationFrameInfo.FrameCount / 30f);

        Stop();
    }

    public void Stop()
    {
        //Reset Frame Values
        SetMaterialProperty("_StartFrame", 0);
        SetMaterialProperty("_EndFrame", 0);
        SetMaterialProperty("_FrameCount", 1);

        //Reset Animation Values
        ApplyMaterial();

        IsPlaying = false;

        CurrentPlayingAnimation = null;
        CurrentPlayingOffset = 0;
    }



    //Set Mesh Renderer Properties
    public void SetMaterialProperty(string propertyName, Color color)
    {
        MaterialPropertyBlock.SetColor(propertyName, color);
    }
    public void SetMaterialProperty(string propertyName, float value)
    {
        MaterialPropertyBlock.SetFloat(propertyName, value);
    }
    public void SetMaterialProperty(string propertyName, Texture newTex)
    {
        MaterialPropertyBlock.SetTexture(propertyName, newTex);
    }


    public void ApplyMaterial()
    {
        Renderer.SetPropertyBlock(MaterialPropertyBlock);
    }



    //Editor Functions
    public void EditorSetup(List<AnimationFrameInfo> frameInformations)
    {
        FrameInformations = frameInformations;
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(DemoGPU_Animator))]
public class CustomInspectorSimpleGPUAnimator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DemoGPU_Animator DemoGPU_Animator = (DemoGPU_Animator)target;


        GUILayout.Space(25);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Play Animation"))
        {
            DemoGPU_Animator.Play(DemoGPU_Animator.ManualAnimationName, 0);
        }
        GUILayout.Space(5);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Play Animation Once"))
        {
            DemoGPU_Animator.PlayOnce(DemoGPU_Animator.ManualAnimationName, 0);
        }

        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Stop Animation"))
        {
            DemoGPU_Animator.Stop();
        }

        EditorGUILayout.LabelField("_____________________________________________________________________________");


    }
}
#endif