using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.AnimatedValues;
using Photon.Pun;

[CustomEditor(typeof(bl_GunManager))]
public class bl_GunManagerEditor : Editor
{
    private AnimBool AssaultAnim;
    protected static bool ShowAssault;
    private AnimBool EnginnerAnim;
    protected static bool ShowEngi;
    private AnimBool ReconAnim;
    protected static bool ShowRecon;
    private AnimBool SupportAnim;
    protected static bool ShowSupport;

    private void OnEnable()
    {
        bl_GunManager script = (bl_GunManager)target;

        AssaultAnim = new AnimBool(ShowAssault);
        AssaultAnim.valueChanged.AddListener(Repaint);
        EnginnerAnim = new AnimBool(ShowEngi);
        EnginnerAnim.valueChanged.AddListener(Repaint);
        ReconAnim = new AnimBool(ShowRecon);
        ReconAnim.valueChanged.AddListener(Repaint);
        SupportAnim = new AnimBool(ShowSupport);
        SupportAnim.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bl_GunManager script = (bl_GunManager)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUILayout.BeginVertical("box");
        DrawNetworkGunsList(script);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        string[] weaponList = bl_GameData.Instance.AllWeaponStringList();

        EditorGUILayout.BeginVertical("box");
        ShowAssault = PhotonGUI.ContainerHeaderFoldout("Assault Class", ShowAssault);
        AssaultAnim.target = ShowAssault;
        if (EditorGUILayout.BeginFadeGroup(AssaultAnim.faded))
        {
            script.m_AssaultClass.primary = EditorGUILayout.Popup("Primary", script.m_AssaultClass.primary, weaponList);
            script.m_AssaultClass.secondary = EditorGUILayout.Popup("Secondary", script.m_AssaultClass.secondary, weaponList);
            script.m_AssaultClass.Knife = EditorGUILayout.Popup("Knife", script.m_AssaultClass.Knife, weaponList);
            script.m_AssaultClass.Special = EditorGUILayout.Popup("Special", script.m_AssaultClass.Special, weaponList);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowEngi = PhotonGUI.ContainerHeaderFoldout("Engineer Class", ShowEngi);
        EnginnerAnim.target = ShowEngi;
        if (EditorGUILayout.BeginFadeGroup(EnginnerAnim.faded))
        {
            script.m_EngineerClass.primary = EditorGUILayout.Popup("Primary", script.m_EngineerClass.primary, weaponList);
            script.m_EngineerClass.secondary = EditorGUILayout.Popup("Secondary", script.m_EngineerClass.secondary, weaponList);
            script.m_EngineerClass.Knife = EditorGUILayout.Popup("Knife", script.m_EngineerClass.Knife, weaponList);
            script.m_EngineerClass.Special = EditorGUILayout.Popup("Special", script.m_EngineerClass.Special, weaponList);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowRecon = PhotonGUI.ContainerHeaderFoldout("Recon Class", ShowRecon);
        ReconAnim.target = ShowRecon;
        if (EditorGUILayout.BeginFadeGroup(ReconAnim.faded))
        {
            script.m_ReconClass.primary = EditorGUILayout.Popup("Primary", script.m_ReconClass.primary, weaponList);
            script.m_ReconClass.secondary = EditorGUILayout.Popup("Secondary", script.m_ReconClass.secondary, weaponList);
            script.m_ReconClass.Knife = EditorGUILayout.Popup("Knife", script.m_ReconClass.Knife, weaponList);
            script.m_ReconClass.Special = EditorGUILayout.Popup("Special", script.m_ReconClass.Special, weaponList);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowSupport = PhotonGUI.ContainerHeaderFoldout("Support Class", ShowSupport);
        SupportAnim.target = ShowSupport;
        if (EditorGUILayout.BeginFadeGroup(SupportAnim.faded))
        {
            script.m_SupportClass.primary = EditorGUILayout.Popup("Primary", script.m_SupportClass.primary, weaponList);
            script.m_SupportClass.secondary = EditorGUILayout.Popup("Secondary", script.m_SupportClass.secondary, weaponList);
            script.m_SupportClass.Knife = EditorGUILayout.Popup("Knife", script.m_SupportClass.Knife, weaponList);
            script.m_SupportClass.Special = EditorGUILayout.Popup("Special", script.m_SupportClass.Special, weaponList);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.SwichTime = EditorGUILayout.Slider("Switch Time", script.SwichTime, 0.1f, 5);
        script.PickUpTime = EditorGUILayout.Slider("Pick Up Time", script.PickUpTime, 0.1f, 5);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.HeadAnimator = EditorGUILayout.ObjectField("Head Animator", script.HeadAnimator, typeof(Animator), allowSceneObjects) as Animator;
        script.TrowPoint = EditorGUILayout.ObjectField("Throw Point", script.TrowPoint, typeof(Transform), allowSceneObjects) as Transform;
        script.SwitchFireAudioClip = EditorGUILayout.ObjectField("Switch Fire Mode Audio", script.SwitchFireAudioClip, typeof(AudioClip), allowSceneObjects) as AudioClip;
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawNetworkGunsList(bl_GunManager script)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("WEAPON MANAGER", EditorStyles.toolbarButton);
        GUILayout.Space(5);
        if (GUILayout.Button(new GUIContent("IMPORT", EditorGUIUtility.IconContent("ol plus").image), EditorStyles.toolbarButton, GUILayout.Width(70)))
        {
            EditorWindow.GetWindow<bl_ImportExportWeapon>("Import", true).PrepareToImport(script.transform.root.GetComponent<bl_PlayerSync>(), null);
        }
        GUILayout.EndHorizontal();
        SerializedProperty listProperty = serializedObject.FindProperty("AllGuns");
        if (listProperty == null)
        {
            return;
        }

        float containerElementHeight = 22;
        float containerHeight = listProperty.arraySize * containerElementHeight;

        bool isOpen = PhotonGUI.ContainerHeaderFoldout("Gun List (" + script.AllGuns.Count + ")", serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue);
        serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue = isOpen;

        if (isOpen == false)
        {
            containerHeight = 0;
        }

        Rect containerRect = PhotonGUI.ContainerBody(containerHeight);
        if (isOpen == true)
        {
            for (int i = 0; i < listProperty.arraySize; ++i)
            {
                Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * i, containerRect.width, containerElementHeight);
                {
                    Rect texturePosition = new Rect(elementRect.xMin + 6, elementRect.yMin + elementRect.height / 2f - 1, 9, 5);              
                   // MFPSEditorUtils.DrawTexture(texturePosition, MFPSEditorUtils.texGrabHandle);
                    Rect propertyPosition = new Rect(elementRect.xMin + 20, elementRect.yMin + 3, elementRect.width - 45, 16);
                    EditorGUI.PropertyField(propertyPosition, listProperty.GetArrayElementAtIndex(i), new GUIContent());

                    Rect removeButtonRect = new Rect(elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        elementRect.yMin + 2,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);

                    GUI.enabled = listProperty.arraySize > 1;
                    if (GUI.Button(removeButtonRect, new GUIContent(MFPSEditorUtils.texRemoveButton), PhotonGUI.DefaultRemoveButtonStyle))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    GUI.enabled = true;

                    if (i < listProperty.arraySize - 1)
                    {
                        texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                        PhotonGUI.DrawSplitter(texturePosition);
                    }
                }
            }
        }

        if (PhotonGUI.AddButton())
        {
            listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();
    }

    GUIStyle FoldOutStyle
    {
        get
        {
            GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            myFoldoutStyle.fontStyle = FontStyle.Bold;
            myFoldoutStyle.fontSize = 14;
            Color myStyleColor = Color.red;
            myFoldoutStyle.normal.textColor = myStyleColor;
            myFoldoutStyle.onNormal.textColor = myStyleColor;
            myFoldoutStyle.hover.textColor = myStyleColor;
            myFoldoutStyle.onHover.textColor = myStyleColor;
            myFoldoutStyle.focused.textColor = myStyleColor;
            myFoldoutStyle.onFocused.textColor = myStyleColor;
            myFoldoutStyle.active.textColor = myStyleColor;
            myFoldoutStyle.onActive.textColor = myStyleColor;
            return myFoldoutStyle;
        }
    }
}