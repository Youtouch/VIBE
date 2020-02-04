using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;

public class ScriptableToolWindow : OdinEditorWindow
{
    public static ScriptableToolWindow window;

    #region Create - Scriptables

    [Button("Create Entity Data", ButtonSizes.Gigantic), GUIColor(.3f, .9f, .3f)]
    public void CreateEntityData()
    {
        InspectObject(ScriptableObjectUtility.CreateAssetAtPath<EntityData>("Assets/Resources/EntityData"));
    }

    [Button("Create Save State", ButtonSizes.Gigantic), GUIColor(.6f, .1f, .4f)]
    public void CreateSaveState()
    {
        InspectObject(ScriptableObjectUtility.CreateAssetAtPath<GameStateData>("Assets/Resources/SaveState"));
    }

    [Button("Create Mini Game Data", ButtonSizes.Gigantic), GUIColor(.1f, .6f, .9f)]
    public void CreateMiniGameData()
    {
        InspectObject(ScriptableObjectUtility.CreateAssetAtPath<MiniGameData>("Assets/Resources/MiniGameData"));
    }

    [Button("Create Item", ButtonSizes.Gigantic), GUIColor(.1f, .4f, .4f)]
    public void CreateItemsData()
    {
        InspectObject(ScriptableObjectUtility.CreateAssetAtPath<ItemData>("Assets/Resources/ItemsData"));
    }

    [Button("Create Buffs", ButtonSizes.Gigantic), GUIColor(.5f, .1f, .9f)]
    public void CreateBuffData()
    {
        InspectObject(ScriptableObjectUtility.CreateAssetAtPath<BuffData>("Assets/Resources/BuffsData"));
    }

    #endregion

    #region WindowFunctions

    [MenuItem("Tools Du Bled/Scriptable Tool")]
    private static void OpenWindow()
    {
        window = GetWindow<ScriptableToolWindow>();
        window.EndWindows();
        window.Show();
    }

    #endregion
}