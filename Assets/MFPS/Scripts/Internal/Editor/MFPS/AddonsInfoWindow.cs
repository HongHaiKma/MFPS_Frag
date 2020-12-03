using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AddonsInfoWindow : EditorWindow
{

    [MenuItem("MFPS/Addons/Info")]
    private static void ShowAboutWindow()
    {
        AddonsInfoWindow window = EditorWindow.GetWindowWithRect<AddonsInfoWindow>(new Rect(100f, 100f, 450f, 470f), true, "Addons Info");//22
        window.position = new Rect(100f, 100f, 450, 470f);
    }

    public void OnGUI()
    {
        GUI.skin.box.richText = true;
        bool isAddons = false;
        string status = "<color=red>No Integrated</color>";
        GUILayout.BeginVertical("box");

        //--------------------------------------------------------------------------------------------------
#if PVOICE
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>";
        GUILayout.Label("<color=white>Photon Voice</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if MFPSM
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ?  "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Mobile Control</color>", "box");  
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton,GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/mfps-mobile-control/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if ULSP
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>ULogin Pro</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/network/ulogin-pro/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
#if CLASS_CUSTOMIZER
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Class Customization</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("http://https//www.lovattostudio.com/en/shop/addons/class-cutomization/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
#if CUSTOMIZER
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Customizer</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/customizer/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
#if KILL_STREAK
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Kill Streak Notifier</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/kill-streak-notifier/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
#if PSELECTOR
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Player Selector</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/player-selector/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if CLANS
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Clan System</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/clan-system/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if INPUT_MANAGER
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Input Manager</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/input-manager-addons/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if BDGM
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Bomb Defuse</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/bomb-defuse-search-and-destroy/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if CP
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Covert Point</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/domination-game-mode-mfps/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if UMM
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>UGUI MiniMap</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/gui/ugui-minimap/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if LM
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Level System</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/level-system/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if GR
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Gun Race</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/gun-race/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
#if LOCALIZATION
        isAddons = true;
#endif
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        status = isAddons ? "<color=green>Integrated</color>" : "<color=red>No Integrated</color>";
        GUILayout.Label("<color=white>Localization</color>", "box");
        GUILayout.Box(string.Format("<color=white>Status:</color> {0}", status));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/localization/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------

        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label("<color=white>HUD Waypoint</color>", "box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Page", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/hud-waypoint-add/");
        }
        GUILayout.EndHorizontal();
        isAddons = false;

        GUILayout.EndVertical();
    }

   /* [MenuItem("MFPS/Create Scriptable")]
    private static void CreateScriptable()
    {
       bl_UtilityHelper.CreateAsset<bl_FootStepsLibrary>();
    }*/
}