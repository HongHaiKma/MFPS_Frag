using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using MFPSEditor;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif

public class PhotonVoiceAddon : MonoBehaviour
{

    private const string DEFINE_KEY = "PVOICE";

    static PhotonVoiceAddon()
    {
       /* int start = PlayerPrefs.GetInt("mfps.addons.define." + DEFINE_KEY, 0);
        if (start == 0)
        {
            bool defines = EditorUtils.CompilerIsDefine(DEFINE_KEY);
            if (!defines)
            {
                EditorUtils.SetEnabled(DEFINE_KEY, true);
            }
            PlayerPrefs.SetInt("mfps.addons.define." + DEFINE_KEY, 1);
        }*/
    }


    [MenuItem("MFPS/Addons/Voice/Enable")]
    private static void Enable()
    {
        bl_GameData.Instance.UseVoiceChat = true;
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }


    [MenuItem("MFPS/Addons/Voice/Enable", true)]
    private static bool EnableValidate()
    {
        return !EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }


    [MenuItem("MFPS/Addons/Voice/Disable")]
    private static void Disable()
    {
        bl_GameData.Instance.UseVoiceChat = false;
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }


    [MenuItem("MFPS/Addons/Voice/Disable", true)]
    private static bool DisableValidate()
    {
        return EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/Voice/Integrate")]
    private static void Instegrate()
    {

#if PVOICE
        GameObject p1 = bl_GameData.Instance.Player1;
        if(p1.GetComponent<PhotonVoiceView>() == null)
        {
            p1.AddComponent<PhotonVoiceView>().UsePrimaryRecorder = true;
        }
        if (p1.GetComponent<Speaker>() == null)
        {
            p1.AddComponent<Speaker>();
        }
        p1 = bl_GameData.Instance.Player2;
        if (p1.GetComponent<PhotonVoiceView>() == null)
        {
            p1.AddComponent<PhotonVoiceView>().UsePrimaryRecorder = true;
        }
        if (p1.GetComponent<Speaker>() == null)
        {
            p1.AddComponent<Speaker>();
        }
        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            string path = "Assets/MFPS/Scenes/MainMenu.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bl_Lobby lb = FindObjectOfType<bl_Lobby>();
            if (lb != null)
            {
                if (FindObjectOfType<PhotonVoiceNetwork>() == null)
                {
                    GameObject nobj = new GameObject("PhotonVoice");
                    PhotonVoiceNetwork pvs = nobj.AddComponent<PhotonVoiceNetwork>();
                    pvs.AutoConnectAndJoin = true;
                    pvs.AutoLeaveAndDisconnect = true;
                    pvs.ApplyDontDestroyOnLoad = true;
                    Recorder r = nobj.AddComponent<Recorder>();
                    r.MicrophoneType = Recorder.MicType.Unity;
                    r.TransmitEnabled = false;
                    r.VoiceDetection = false;
                    pvs.PrimaryRecorder = r;
                    nobj.AddComponent<bl_PhotonAudioDisabler>().isGlobal = true;
                    EditorUtility.SetDirty(nobj);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                Debug.Log("Photon Voice Integrated, enable it on GameData.");
            }
            else
            {
                Debug.Log("Can't found Menu scene.");
            }
        }
        else
        {
            Debug.LogWarning("Can't complete the integration of the addons because MFPS folder structure has been change, please do the manual integration.");
        }
#else
        Debug.LogWarning("Enable Photon Voice addon before integrate.");
#endif
    }

    [MenuItem("MFPS/Addons/Voice/Package")]
    private static void OpenPackagePage()
    {
        AssetStore.Open("content/130518");
    }
}