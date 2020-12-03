using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class bl_TouchHelper : MonoBehaviour
{

    [SerializeField] private GameObject PushToTalkButton;

    private static bl_TouchHelper m_Instance;
    private static int m_Touch = -1;
    private static List<int> m_Touches;

    public delegate void ButtonEvent();
    public static ButtonEvent OnFireClick;
    public static ButtonEvent OnReload;
    public static ButtonEvent OnCrouch;
    public static ButtonEvent OnJump;
    public static ButtonEvent OnKit;
    public static ButtonEvent OnPause;

    public delegate void BoolEvent(bool b);
    public static BoolEvent OnTransmit;

    public bool FireDown { get; set; }
    public bool isAim { get; set; }
    private Canvas m_Canvas;
    private bl_ChatRoom Chat;

    private void Awake()
    {
        this.ActiveIgnoreTouches = new List<int>();
        m_Canvas = transform.parent.GetComponent<Canvas>();
        Chat = FindObjectOfType<bl_ChatRoom>();
        bl_EventHandler.OnLocalPlayerSpawn += OnSpawn;
        m_Canvas.enabled = false;
    }

    public void OnChat()
    {
        Chat.SetChat();
    }

    private void OnDestroy()
    {
        bl_EventHandler.OnLocalPlayerSpawn -= OnSpawn;
    }

    void OnSpawn()
    {
        m_Canvas.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnFireClicked()
    {
        if (OnFireClick != null)
        {
            OnFireClick.Invoke();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnReloadClicked()
    {
        if (OnReload != null)
        {
            OnReload.Invoke();
        }
    }

    public void OnCrouchClicked()
    {
        if (OnCrouch != null)
        {
            OnCrouch.Invoke();
        }
    }

    public void OnJumpClicked()
    {
        if (OnJump != null)
        {
            OnJump.Invoke();
        }
    }

    public void OnKitClicked()
    {
        if (OnKit != null)
        {
            OnKit.Invoke();
        }
    }

    public void OnPauseClick()
    {
        if(OnPause != null)
        {
            OnPause.Invoke();
        }
    }

    public void EnableMicro(bool yes)
    {
        if(OnTransmit != null)
        {
            OnTransmit.Invoke(yes);
        }
    }

    public void OnPushToTalkChange(bool active)
    {
        PushToTalkButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAim()
    {
        isAim = !isAim;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static int GetUsableTouch()
    {
        if (Input.touches.Length <= 0)
        {
            m_Touch = -1;
            return m_Touch;
        }
        List<int> list = GetValuesFromTouches(Input.touches).Except<int>(Instance.ActiveIgnoreTouches).ToList<int>();
        if (list.Count <= 0)
        {
            m_Touch = -1;
            return m_Touch;
        }
        if (!list.Contains(m_Touch))
        {
            m_Touch = list[0];
        }
        return m_Touch;
    }

    public static List<int> GetValuesFromTouches(Touch[] touches)
    {
        if (m_Touches == null)
        {
            m_Touches = new List<int>();
        }
        else
        {
            m_Touches.Clear();
        }
        for (int i = 0; i < touches.Length; i++)
        {
            m_Touches.Add(touches[i].fingerId);
        }
        return m_Touches;
    }

    public List<int> ActiveIgnoreTouches { get; private set; }

    public static Quaternion ClampRotationAroundXAxis(Quaternion q, float min, float max)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1f;
        float num = 114.5916f * Mathf.Atan(q.x);
        num = Mathf.Clamp(num, min, max);
        q.x = Mathf.Tan(0.008726646f * num);
        return q;
    }


    public static bl_TouchHelper Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(bl_TouchHelper)) as bl_TouchHelper;
            }
            return m_Instance;
        }
    }
}