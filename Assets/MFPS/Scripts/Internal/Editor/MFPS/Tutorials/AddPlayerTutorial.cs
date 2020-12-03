using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class AddPlayerTutorial : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/player/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "dDHltGGDrAA", Image = null, Type = NetworkImages.ImageType.Youtube},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "https://i1.wp.com/www.lovattostudio.com/en/wp-content/uploads/2017/03/player-selector-product-cover.png?fit=925%2C484",Type = NetworkImages.ImageType.Custom},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "3DModel", StepsLenght = 0 },
    new Steps { Name = "Ragdolled", StepsLenght = 3 },
    new Steps { Name = "Player Prefab", StepsLenght = 6 },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private GameObject PlayerModel;
    private Animator PlayerAnimator;
    private Avatar PlayerModelAvatar;
    private string LogLine = "";
    private ModelImporter ModelInfo;
    Editor p1editor;

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
        if (window == 0)
        {
            DrawModelInfo();
        }
        else if (window == 1)
        {
            DrawRagdolled();
        }
        else if (window == 2)
        {
            DrawPlayerPrefab();
        }
    }

    void DrawModelInfo()
    {
        DrawText("This tutorial will guide you step by step to replace the player model of one of the player prefabs, for it you need:");
        DrawHorizontalColumn("Player Model", "A Humanoid <b>Rigged</b> 3D Model with the standard rigged bones or any rigged that work with the unity re-targeting animator system.");
        DownArrow();
        DrawText("So when you have the player model imported in your project, select it in the Project Window (the model, not a prefab) and under the 'Rig' settings set up like this:");
        DownArrow();
        DrawImage(GetServerImage(0));
        DownArrow();
        DrawText("Important: your model should be in T-Pose to work correctly with the re-targeting animations, if your character model is not in T-Pose you can follow this video tutorial for " +
            "fix it:");
        DownArrow();
        DrawYoutubeCover("Adjusting Avatar for correct animation retargeting", GetServerImage(1), "https://www.youtube.com/watch?v=dDHltGGDrAA");
    }

    void DrawRagdolled()
    {
        if (subStep == 0)
        {
            HideNextButton = true;
            DrawText("Alright, with the model ready it's time to start set up it, the first thing that you need to do is make a ragdoll of your new player model," +
                " So normally you make a ragdoll manually with GameObject -> " +
                "3D Object -> Ragdoll -> and then assign every player bone in the wizard window manually, but this tool will make this automatically, you simple need drag the player model.");
            DownArrow();
            DrawText("Drag here your player model from the <b>Project View</b>");
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), false) as GameObject;
            GUI.enabled = PlayerModel != null;
            if (DrawButton("Continue"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PlayerModel));
                if (importer != null)
                {
                    ModelInfo = importer as ModelImporter;
                    if (ModelInfo != null)
                    {
                        if (ModelInfo.animationType == ModelImporterAnimationType.Human)
                        {
                            PlayerInstantiated = PrefabUtility.InstantiatePrefab(PlayerModel) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(PlayerInstantiated, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                            PlayerAnimator = PlayerInstantiated.GetComponent<Animator>();
                            PlayerModelAvatar = PlayerAnimator.avatar;
                            var view = (SceneView)SceneView.sceneViews[0];
                            view.LookAt(PlayerInstantiated.transform.position);
                            EditorGUIUtility.PingObject(PlayerInstantiated);
                            Selection.activeTransform = PlayerInstantiated.transform;
                            subStep++;
                        }
                        else
                        {
                            LogLine = "Your model is not set to Humanoid Rig!";
                        }
                    }
                    else
                    {
                        LogLine = "Please Select Imported Model in Project View not prefab or other.";
                    }
                }
                else { LogLine = "Please Select Imported Model in Project View not prefab"; }
            }
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(LogLine))
            {
                GUILayout.Label(LogLine);
            }
        }
        else if (subStep == 1)
        {
            HideNextButton = false;
            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), false) as GameObject;
            PlayerModelAvatar = EditorGUILayout.ObjectField("Avatar", PlayerModelAvatar, typeof(Avatar), true) as Avatar;
            SkinnedMeshRenderer smr = null;
            if (PlayerInstantiated != null)
            {
                smr = PlayerInstantiated.GetComponentInChildren<SkinnedMeshRenderer>();
                GUILayout.Label(string.Format("Model Height: <b>{0}</b> | Expected Height: <b>2</b>", smr.bounds.size.y));
                if(ModelInfo!=null)
                GUILayout.Label(string.Format("Model Rig: {0}", ModelInfo.animationType.ToString()));
                GUI.enabled = true;
                if (smr.bounds.size.y < 1.9f)
                {
                    GUILayout.Label("<color=yellow>the size of the model is very small</color>, you want resize it automatically?", EditorStyles.label);
                    if (DrawButton("Yes, Resize automatically"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = 2f / smr.bounds.size.y;
                        v = v * dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }else if(smr.bounds.size.y > 2.25f)
                {
                    GUILayout.Label("<color=yellow>the size of the model is very large</color>, you want resize it automatically?", EditorStyles.label);
                    if (DrawButton("Yes, Resize automatically"))
                    {
                        Vector3 v = PlayerInstantiated.transform.localScale;
                        float dif = smr.bounds.size.y / 2;
                        v = v / dif;
                        PlayerInstantiated.transform.localScale = v;
                    }
                }
            }
            GUILayout.EndVertical();
            GUI.enabled = true;
            if(PlayerModelAvatar != null && PlayerAnimator != null)
            {
                DownArrow();
                DrawText("All ready to create the ragdoll, Click in the button below to build it.");
                if(DrawButton("Build Ragdoll"))
                {
                    if (AutoRagdoller.Build(PlayerAnimator))
                    {
                        var view = (SceneView)SceneView.sceneViews[0];
                        view.ShowNotification(new GUIContent("Ragdoll Created!"));
                        NextStep();
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=yellow>Hmm... something is happening here, can't get the model avatar.</color>", EditorStyles.label);
            }
        }
        else if(subStep == 2)
        {
            DrawText("Right now your player model (in the scene) should look like this:");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Now the ragdoll is ready, what you need to do now is create a prefab of this ragdoll (don't delete the one in the scene yet), simple drag the player model from the hirarchy window " +
                "into a folder in the Project Window, just remember in which folder cuz you will need that prefab later.\n\nYou are ready for the next step.");

        }
    }

    void DrawPlayerPrefab()
    {
        if (subStep == 0)
        {
            DrawText("Ok, so to continue with the integration of the player, now we already have the player model ragdolled, so we can insert in one of the player prefabs," +
                " for it we need to open the player prefab in which you want add this model, for default there are only 2 player prefabs, 1 for each team, so we need make a instance of one.\n\n" +
                "Select the Player Prefab in which you wanna add this player model:");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Player 1", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                PlayerModel = PlayerInstantiated;
                PlayerInstantiated = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.Player1) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(PlayerInstantiated, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                Selection.activeObject = PlayerInstantiated;
                EditorGUIUtility.PingObject(PlayerInstantiated);
                NextStep();
            }
            GUILayout.Label(AssetPreview.GetAssetPreview(bl_GameData.Instance.Player1));
            GUILayout.EndVertical();
            GUILayout.Label("Or", GUILayout.Width(25));
            GUILayout.BeginVertical();
            if (GUILayout.Button("Player 2", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                PlayerModel = PlayerInstantiated;
                PlayerInstantiated = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.Player2) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(PlayerInstantiated, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
                Selection.activeObject = PlayerInstantiated;
                EditorGUIUtility.PingObject(PlayerInstantiated);
                NextStep();
            }
            GUILayout.Label(AssetPreview.GetAssetPreview(bl_GameData.Instance.Player2));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if (subStep == 1)
        {
            GUI.enabled = (PlayerInstantiated == null || PlayerModel == null);
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), true) as GameObject;
            if (PlayerModel == null)
            {
                GUILayout.Label("<color=yellow>Select the ragdolled player model (from hierarchy)</color>");
            }
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), true) as GameObject;
            GUI.enabled = true;
            if (PlayerModel != null && PlayerInstantiated != null)
            {
                DownArrow();
                DrawText("All good, click in the button below to setup the model in the player prefab.");
                GUILayout.Space(5);
                if(DrawButton("SETUP MODEL"))
                {
                    SetUpModelInPrefab();
                    NextStep();
                }
            }
        }
        else if (subStep == 2)
        {
            DrawText("if all work as expected you should see a notification in the scene view, you can confirm simply see if the new player model is inside of the player prefab " +
                "in the hierarchy, like this:");
            DrawImage(GetServerImage(3));
            DownArrow();
            DrawText("So that mean that the player model has been integrated in the player prefab successfully, now there a step that you need to do manually, the TPWeapons has been moved" +
                " from the old player model to the new one but due the models normally have a different local axis orientation, the TPWeapon will be in a wrong position, so you need repositioned it," +
                "don't worry you don't need to do for each weapon, just the parent transform 'RemoteWeapons'.\n\n so this is a example of how the weapons will be by default after replace the player model:");
            DrawImage(GetServerImage(4));
            DownArrow();
            DrawText("So for repositioned it, select the 'RemoteWeapons' object inside of the player prefab (in the right hand of the player model), or click in the button bellow to try ping automatically.");
            if(DrawButton("Ping RemoteWeapons"))
            {
                if(PlayerInstantiated != null)
                {
                    Transform t = PlayerInstantiated.GetComponent<bl_PlayerSync>().NetworkGuns[0].transform.parent;
                    Selection.activeTransform = t;
                    EditorGUIUtility.PingObject(t);
                    NextStep();
                }
            }

        }
        else if(subStep == 3)
        {
            DrawText("Now the RemoteWeapons object should be selected and focus, so for preview the position of the weapons if you don't have a weapon active," +
                "select one from inside of the object (RemoteWeapons) and active it, then select again the RemoteWeapons transform and rotate / positioned it " +
               "simulating that is holding by the right hand.");
            DrawImage(GetServerImage(5));
            DownArrow();
            DrawText("Good, now if you want, you can delete the old model from the prefab, it is marked with '[DELETE THIS]' text");
            DrawImage(GetServerImage(6));
        }
        else if(subStep == 4)
        {
            DrawText("Then you need create a new prefab of this, but you need create it inside a <b>Resources</b> folder, so you can drag in: MFPS -> Resources");
            DrawImage(GetServerImage(7));
            DownArrow();
            DrawText("Now you need assign this new player prefab for use in one of the Team (team 1 or team 2), for it go to GameData (in Resources folder too) -> Players section, and in the correspondent field (Team1 or Team2) " +
                "drag the new player prefab");
            DrawImage(GetServerImage(8));
        }
        else if(subStep == 5)
        {
            DrawText("That's you have your new player model integrated!.\n\n Here some notes, some models are not fully compatible with the default player animations re targeting causing " +
                "that some animations look awkward, unfortunately there is nothing that we can do to fix it automatically, for fix it you have two options: Edit the animation or replace with other that you know" +
                " that work in your model, check the documentation for more info of how replace animations.");
            GUILayout.Space(7);
            DrawText("You want to have multiple player options so player can select the player with which they want to play?, Check out <b>Player Selector</b> Addon: ");
            GUILayout.Space(5);
            if(DrawButton("PLAYER SELECTOR"))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/player-selector/");
            }
            DrawImage(GetServerImage(9));
        }
    }

    void SetUpModelInPrefab()
    {
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(PlayerModel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        GameObject TempPlayerPrefab = PlayerInstantiated;
        GameObject TempPlayerModel = PlayerModel;

        //change name of prefabs to identify
        PlayerInstantiated.gameObject.name += " [NEW]";
        PlayerModel.name += " [NEW]";

        // get the current player model
        GameObject RemoteChildPlayer = TempPlayerPrefab.GetComponentInChildren<bl_PlayerAnimations>().gameObject;
        GameObject ActualModel = TempPlayerPrefab.GetComponentInChildren<bl_PlayerIK>().gameObject;
        Transform NetGunns = TempPlayerPrefab.GetComponent<bl_PlayerSync>().NetworkGuns[0].transform.parent;

        //set the new model to the same position as the current model
        TempPlayerModel.transform.parent = RemoteChildPlayer.transform;
        TempPlayerModel.transform.localPosition = ActualModel.transform.localPosition;
        TempPlayerModel.transform.localRotation = ActualModel.transform.localRotation;

        //add and copy components of actual player model
        bl_PlayerIK ahl = ActualModel.GetComponent<bl_PlayerIK>();
        if (TempPlayerModel.GetComponent<Animator>() == null) { TempPlayerModel.AddComponent<Animator>(); }
        Animator NewAnimator = TempPlayerModel.GetComponent<Animator>();

        if (ahl != null)
        {

            bl_PlayerIK newht = TempPlayerModel.AddComponent<bl_PlayerIK>();
            newht.Target = ahl.Target;
            newht.Body = ahl.Body;
            newht.Weight = ahl.Weight;
            newht.Head = ahl.Head;
            newht.Lerp = ahl.Lerp;
            newht.Eyes = ahl.Eyes;
            newht.Clamp = ahl.Clamp;
            newht.useFootPlacement = ahl.useFootPlacement;
            newht.FootDownOffset = ahl.FootDownOffset;
            newht.FootHeight = ahl.FootHeight;
            newht.FootLayers = ahl.FootLayers;
            newht.PositionWeight = ahl.PositionWeight;
            newht.RotationWeight = ahl.RotationWeight;
            newht.AimSightPosition = ahl.AimSightPosition;
            newht.HandOffset = ahl.HandOffset;
            newht.TerrainOffset = ahl.TerrainOffset;
            newht.Radious = ahl.Radious;

            Animator oldAnimator = ActualModel.GetComponent<Animator>();
            NewAnimator.runtimeAnimatorController = oldAnimator.runtimeAnimatorController;
            NewAnimator.applyRootMotion = oldAnimator.hasRootMotion;
            if (NewAnimator.avatar == null)
            {
                NewAnimator.avatar = oldAnimator.avatar;
                Debug.LogWarning("Your new model doesn't have a avatar, that can cause some problems with the animations, be sure to add it manually.");
            }
        }
        Transform RightHand = NewAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (RightHand == null)
        {
            Debug.Log("Can't get right hand from new model, are u sure that is an humanoid rig?");
            return;
        }

        bl_PlayerAnimations pa = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerAnimations>();
        bl_BodyPartManager bdm = TempPlayerPrefab.transform.GetComponentInChildren<bl_BodyPartManager>();
        pa.m_animator = NewAnimator;
        bdm.m_Animator = NewAnimator;
        bdm.GetRequireBones();

        bdm.mRigidBody.Clear();
        bdm.HitBoxs.Clear();
        bdm.mRigidBody.AddRange(GetRigidBodys(TempPlayerModel.transform));
        Collider[] allColliders = GetCollider(TempPlayerModel.transform);
        if (allColliders == null || allColliders.Length <= 0)
        {
            Debug.Log("New player model prefab is not rag-dolled, to continue please create a rag-doll of it.");
            return;
        }
        foreach (Collider c in allColliders)
        {
            if (c.gameObject.tag != bl_BodyPartManager.HitBoxTag)
            {
                c.gameObject.tag = bl_BodyPartManager.HitBoxTag;
            }
            bl_BodyPartManager.Part p = new bl_BodyPartManager.Part();
            p.m_Collider = c;
            p.name = c.name;
            c.gameObject.layer = LayerMask.NameToLayer("Player");
            if (c.gameObject.name.ToLower().Contains("head"))
            {
                p.m_HeatShot = true;
                p.m_Multipler = 50;
            }
            bdm.HitBoxs.Add(p);
        }
        bdm.AddScript();

        if (RightHand != null)
        {
            NetGunns.parent = RightHand;
            NetGunns.localPosition = Vector3.zero;
            NetGunns.rotation = RightHand.rotation;
        }
        else
        {
            Debug.Log("Can't find right hand");
        }

        ActualModel.name += " (DELETE THIS)";
        ActualModel.SetActive(false);

        var view = (SceneView)SceneView.sceneViews[0];
        view.LookAt(ActualModel.transform.position);
        view.ShowNotification(new GUIContent("Player Setup"));
    }

    private Rigidbody[] GetRigidBodys(Transform t)
    {
        Rigidbody[] R = t.GetComponentsInChildren<Rigidbody>();
        return R;
    }

    private Collider[] GetCollider(Transform t)
    {
        Collider[] R = t.GetComponentsInChildren<Collider>();
        return R;
    }

    [MenuItem("MFPS/Tutorials/Add Player")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddPlayerTutorial));
    }
}