using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;

public class AddMapTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/map/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 0 },
    new Steps { Name = "Set Up Scene", StepsLenght = 6 },
    new Steps { Name = "Tips", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////
    private Object m_SceneReference;
    string[] RequiredsPaths = new string[]
    {
        "Assets/MFPS/Content/Prefabs/Network/GameManager.prefab",
        "Assets/MFPS/Content/Prefabs/GamePlay/AIManager.prefab",
        "Assets/MFPS/Content/Prefabs/GamePlay/ItemManager.prefab",
        "Assets/MFPS/Content/Prefabs/GamePlay/CTFMode.prefab",
        "Assets/MFPS/Content/Prefabs/UI/UI.prefab",
    };
    bool[] RequiredInstanced = new bool[] { false, false, false, false, false,};
    string sceneName = "";
    Sprite scenePreview = null;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
       if(window == 0)
        {
            DrawStarted();
        }else if(window == 1) { DrawSetup(); }
        else if (window == 2) { DrawTips(); }
    }

    void DrawStarted()
    {
        DrawText("In this tutorial you will learn how to add a new map, in order to be available to play with MFPS.");
        DownArrow();
        DrawText("First, what you need is of course a map, with a map I mean all the art content models, prefabs, light, sky, etc... placed in a way that looks like a battlefield.");
        DrawText("There are some basic requirement for the map that everybody that have use unity know, that apply for all games not just MFPS, but lets mentioned it just to be sure,\n \n" +
            "-<b>The map models / prefabs must have a collider</b>, except for the models that are only as decoration, all models need have a collider in order to player not pass through them.\n \n" +
            "-<b>Lighting</b>, lighting is a important part of map and also affect in a big way the performance of game, there a tons of tutorial out there of how build a good scene lighting. \n \n" +
            "-<b>Optimization over good looking</b>, we all love good graphics in a game, but a bad optimized level could kill your game, now MFPS is well optimized in which code and logic refer," +
            " but most import than the code (at least in this case) is the graphic optimization, there is a good Unity Official post for graphic optimization, check it out: ");
        if (DrawLinkText("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html");
        }
        DownArrow();
        DrawText("All right, if you have you map level design ready, lets continue");
    }

    void DrawSetup()
    {
        if (subStep == 0)
        {
            DrawText("Like I said before you need have your new map scene design ready, but not just as a prefab, you need have it placed in a Unity Scene (.scene)," +
                "if you don't have it, if you have only the map design as a prefab or don't have design it yet, create a new Scene in <b>File -> New Scene</b>, in the opened scene place your map design or design it from scratch." +
                "\n once you have it, save the scene in your Unity Project (File -> Save Scenes).\n \n" +
                "Be sure that your map scene don't have any camera cuz you won't use it.");
            DrawImage(GetServerImage(0));
            DownArrow();
            DrawText("Ok, so now lets open the new map scene, <B>drag your scene in the field below</b>");
            GUILayout.BeginHorizontal();
            GUILayout.Label("SCENE: ", GUILayout.Width(100));
            m_SceneReference = EditorGUILayout.ObjectField(m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUILayout.EndHorizontal();
            GUI.enabled = m_SceneReference != null;
            Space(5);
            if (DrawButton("CONTINUE"))
            {
                if (EditorSceneManager.GetActiveScene().name == m_SceneReference.name)
                {
                    NextStep();
                }
                else
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    NextStep();
                }
            }
            GUI.enabled = true;
        }
        else if (subStep == 1)
        {
            HideNextButton = isMissing();
            DrawText("Ok, now with the map scene open, let's drag the objects needed for the scene work with MFPS, but first let's check which assets you have in the scene already, click on the button bellow" +
                " to auto detect.");
            Space();
            if (DrawButton("Check Scene"))
            {
                CheckScene();
            }
            if (sceneChecked)
            {
                EditorStyles.helpBox.richText = true;
                GUILayout.BeginVertical("box");
                GUILayout.Label(string.Format("Game Manager: {0}", RequiredInstanced[0] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("AI Manager: {0}", RequiredInstanced[1] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("Item Manager: {0}", RequiredInstanced[2] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("CTF Objects: {0}", RequiredInstanced[3] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.Label(string.Format("UI: {0}", RequiredInstanced[4] ? "<color=green>YES</color>" : "<color=red>NO</color>"), EditorStyles.helpBox);
                GUILayout.EndVertical();

                if (isMissing())
                {
                    DrawText("Your scene is not properly setup yet, lets do it, click on the button bellow");
                    Space();
                    if (DrawButton("Setup scene"))
                    {
                        for (int i = 0; i < RequiredsPaths.Length; i++)
                        {
                            Debug.Log(RequiredsPaths[i]);
                            if (RequiredInstanced[i] || RequiredsPaths[i] == string.Empty) continue;
                            GameObject prefab = AssetDatabase.LoadAssetAtPath(RequiredsPaths[i], typeof(GameObject)) as GameObject;
                            if (prefab != null)
                            {
                                PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene());
                                Debug.Log("instanced: " + prefab.name);
                            }
                            else
                            {
                                Debug.LogWarning("Could not find the prefab at: " + RequiredsPaths[i]);
                            }
                        }
                        CheckScene();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Repaint();
                    }
                }
                else
                {
                    DrawText("All good, your scene have all require objects, continue with the next step.");
                }
            }
        }
        else if (subStep == 2)
        {
            DrawText("Ok, now after instanced the MFPS objects you will see in the Game View that there is a camera render from the top, that's camera that show up when a player enter in the room for select the team to join," +
                " so positioned it to your tasted in a space where render a good preview of your map.\nThe camera is located in <i>(Hierarchy) GameManager -> Camera</i>");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("There are some objects that you need repositioned in your map, one of they are the CTF flags, these are inside the object <b>CTFMode</b> in hierarchy," +
                " so select they and positioned it where you want they to be.");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Do the same with the objects under <b>ItemManager</b> positioned they around the map, but these are optional, you can use or not, these items (med kits and ammo) can be used as permanent items around the map " +
                "so player can use them during game, if you don't wanna use them simply delete it from the scene.");
            DrawImage(GetServerImage(3));
        }
        else if (subStep == 3)
        {
            DrawText("Now, you need create some <b>Spawn Points</b> for each Team (Team 1, Team 2 and For FFA), these spawn points are not specifically a static position, they are a <b>Spherical Area</b> " +
                "where players can spawn, that's mean that the player will be instantiated in a random position inside of the radius of the area.\n \n<b><i>So how create spawn points?</i></b>\nSimple create a empty game object in the scene" +
                " and add the script <b>bl_SpawnPoint.cs</b> to it and assign the area and the team, but lets make this more easy for you, bellow you will have a button to create a spawn point, simple select the team " +
                "and click on the button <b> Create Spawn Point</b> after this the spawn point will be selected on the scene view and you can positioned it in the map");
            Space();
            GUILayout.BeginVertical();
            DrawText("<color=yellow>CREATE SPAWN POINTS</color>");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Spawn Point For: ");
            SpawnTeam = (Team)EditorGUILayout.EnumPopup(SpawnTeam);
            if (GUILayout.Button("Create Spawn Point", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                CreateSpawnPoint();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            DownArrow();
            DrawText("Once you create a spawn point and positioned it you will have something similar to this:");
            DrawImage(GetServerImage(4));
            DrawText("This is the spawn point area, you can preview the available area to spawn a player inside with the semi-circle and the real player size.\n \n you can increase or decrease the area in the bl_Spawnpoint " +
                "script attached to the object -> <b>Spawn Space</b>, also you can rotate to set the default rotation (where player will spawn look at in).\n \nbe careful that the feet of the player gizmo and the sphere area are over the floor," +
                " otherwise the players will fall down when spawn.");
            DrawText("\nCreate many spawn point as you want using the above button, just make sure to create at least a spawn point for each team, once you finish, proceed with the next step.");
        }
        else if(subStep == 4)
        {
            DrawText("Now in order to AI agents work on this map you need to bake the <b>Navmesh</b>, in resume a navmesh is the area where AI Agents can move on, this is calculated based in static mesh with a collider" +
                " in your scene, for more info and more detailed explanation of how bake a Navmesh check this: ");
            if (DrawLinkText("https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html"))
            {
                Application.OpenURL("https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html");
            }
            DownArrow();
            DrawText("Once you bake your Navmesh you will have something like this:");
            DrawImage(GetServerImage(5));
            DrawText("the blue gizmo represent the walkable area for bots\n \nSo now you need to positioned the <b>AI Covert Points</b>, those are a empty game object with a bl_AICoverPoint script attached, that work" +
                " as reference position for bots when they need to cover, by default the AIManager object contains some of those points, you can find they in <b>AIManager -> *</b>");
            DrawImage(GetServerImage(6),TextAlignment.Center);
            DrawText("You can use as many as you want, if don't need that much simply delete some objects, if you want more simply duplicated one.\n" +
                "So select each individual and positioned in a strategic point in the map, you can preview the area enabling <b>Show Gizmos</b> in <i>AIManager -> bl_AIManager -> Show Gizmos</i>");
            DrawImage(GetServerImage(7));
        }
        else if(subStep == 5)
        {
            DrawText("Nice, now you have your scene set up ready, now you need to listed it in the available scene list in order to players can select this scene when create a room " +
                " for it you can do it manually adding a new field in the list <b>AllScenes</b> in GameData: <b><i>(Resources folder of MFPS) GameData -> AllScenes -> Add a new field</i></b> and fill all the require info\nor you can do it automatically here," +
                " Simple set a Name for the map and a sprite preview.");
            DrawImage(GetServerImage(8));
            DownArrow();
            GUILayout.BeginVertical("box");
            sceneName = EditorGUILayout.TextField("Map Custom Name", sceneName);
            scenePreview = EditorGUILayout.ObjectField("Map Preview", scenePreview, typeof(Sprite), false) as Sprite;
            GUI.enabled = m_SceneReference == null;
            m_SceneReference = EditorGUILayout.ObjectField("Scene",m_SceneReference, typeof(SceneAsset), false) as SceneAsset;
            GUI.enabled = !string.IsNullOrEmpty(sceneName) && m_SceneReference != null;
            if(DrawButton("List Map"))
            {
                if(!bl_GameData.Instance.AllScenes.Exists(x => x.ShowName == sceneName))
                {
                    bl_GameData.SceneInfo si = new bl_GameData.SceneInfo();
                    si.ShowName = sceneName;
                    si.m_Scene = m_SceneReference;
                    si.Preview = scenePreview;
                    bl_GameData.Instance.AllScenes.Add(si);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    var original = EditorBuildSettings.scenes;
                    var newSettings = new EditorBuildSettingsScene[original.Length + 1];
                    System.Array.Copy(original, newSettings, original.Length);
                    string path = AssetDatabase.GetAssetPath(m_SceneReference);
                    var sceneToAdd = new EditorBuildSettingsScene(path, true);
                    newSettings[newSettings.Length - 1] = sceneToAdd;
                    EditorBuildSettings.scenes = newSettings;
                    sceneListed = true;
                }
                else
                {
                    Debug.LogWarning("A map with this name is already listed.");
                }
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            if (sceneListed)
            {
                DownArrow();
                DrawText("Nice!, that's, you have added a new map to your game!, now you can select the map in the Main Menu / Lobby -> Create Room.\n \nRead the next section for some tips.");
                DrawImage(GetServerImage(9));
            }
        }
    }

    void DrawTips()
    {
        DrawText("As I mentioned at the begging, if you want to your game performed well, that have a good frame rate when there is more than 8 players in the same map, " +
            "you need to do focus in the graphic content of your map, Models, Textures, Shaders, etc..., at difference of a single player game, in multiplayer the local client handle with their local info " +
            "and the info receive from remote players so the optimization is really, really important.\nbellow will leave you some link to useful post for Unity Graphic Optimization");
        DownArrow();
        if(DrawLinkText("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html");
        }
        Space(10);
        if (DrawLinkText("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games"))
        {
            Application.OpenURL("https://unity3d.com/es/learn/tutorials/temas/performance-optimization/optimizing-graphics-rendering-unity-games");
        }
        Space(10);
        if (DrawLinkText("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html"))
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html");
        }
        Space(10);
        if (DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance"))
        {
            Application.OpenURL("https://cgcookie.com/articles/maximizing-your-unity-games-performance");
        }
    }

    Team SpawnTeam = Team.All;
    bool sceneChecked = false;
    bool sceneListed = false;
    void CheckScene()
    {
        RequiredInstanced[0] = FindObjectOfType<bl_GameManager>() != null;
        RequiredInstanced[1] = FindObjectOfType<bl_AIMananger>() != null;
        RequiredInstanced[2] = FindObjectOfType<bl_ItemManager>() != null;
        RequiredInstanced[3] = GameObject.Find("CTFMode") != null;
        RequiredInstanced[4] = FindObjectOfType<bl_UIReferences>() != null;
        sceneChecked = true;
    }

    void CreateSpawnPoint()
    {
        GameObject parent = GameObject.Find("SpawnPoints");
        if (parent == null)
        {
            parent = new GameObject("SpawnPoints");
            parent.transform.position = Vector3.zero;
        }
        if (SpawnTeam == Team.Delta)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team1Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team1Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.m_Team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else if (SpawnTeam == Team.Recon)
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", bl_GameData.Instance.Team2Name));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", bl_GameData.Instance.Team2Name));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.m_Team = SpawnTeam;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
        else
        {
            GameObject t1p = GameObject.Find(string.Format("{0} Spawnpoints", "ALL"));
            if (t1p == null)
            {
                t1p = new GameObject(string.Format("{0} Spawnpoints", "ALL"));
                t1p.transform.parent = parent.transform;
                t1p.transform.localPosition = Vector3.zero;
            }
            GameObject spawn = new GameObject(string.Format("SpawnPoint [{0}]", "ALL"));
            bl_SpawnPoint sp = spawn.AddComponent<bl_SpawnPoint>();
            sp.m_Team = Team.All;
            spawn.transform.parent = t1p.transform;
            Selection.activeObject = spawn;
            EditorGUIUtility.PingObject(spawn);
            var view = (SceneView)SceneView.sceneViews[0];
            spawn.transform.position = view.camera.transform.position + view.camera.transform.forward * 10;
        }
    }

    private bool isMissing()
    {
        for (int i = 0; i < RequiredInstanced.Length; i++)
        {
            if (RequiredInstanced[i] == false) return true;
        }
        return false;
    }

    [MenuItem("MFPS/Tutorials/Add Map")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddMapTutorial));
    }
}