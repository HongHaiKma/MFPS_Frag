//////////////////////////////////////////////////////////////////////////////
//  bl_DrawName.cs
//
// Can be attached to a GameObject to show Player Name 
//
//           Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif
//[ExecuteInEditMode]
public class bl_DrawName : bl_MonoBehaviour
{
    [Header("Settings")]
    public bool isBot = false;
    [SerializeField, Range(5, 30)]
    private float LargeIconSize = 15;
    [SerializeField, Range(0.1f, 25)] private float HealthBarHeight = 10;
    [SerializeField] private Color HealthBackColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Color HealthBarColor = new Color(0, 1, 0, 0.9f);

    [Header("References")]
    public GUIStyle m_Skin;
    /// <summary>
    /// at what distance the name is hiding
    /// </summary>
    public float m_HideDistance;
    /// <summary>
    /// 
    /// </summary>    
    public Texture2D m_HideTexture;
    public Texture2D TalkingIcon;
    [SerializeField] private Texture2D BarTexture;
    // [Range(0, 100)] public float Health = 100;
    public string m_PlayerName { get; set; }
    public Transform m_Target;

    //Private
    private float m_dist;
    private Transform myTransform;
#if !UNITY_WEBGL && PVOICE
    private PhotonVoiceView PRecorder;
    private float RightPand = 0;
    private float NameSize = 0;
#endif
    private bl_PlayerDamageManager DamagerManager;
    private bl_AIShooterHealth AIHealth;
    private bool ShowHealthBar = false;
    private bool isFinish = false;

    protected override void Awake()
    {
        base.Awake();
#if !UNITY_WEBGL && PVOICE
        if (!isBot)
        {
            PRecorder = GetComponent<PhotonVoiceView>();
        }
#endif
        if (!isBot)
        {
            DamagerManager = GetComponent<bl_PlayerDamageManager>();
        }
        else
        {
            AIHealth = GetComponent<bl_AIShooterHealth>();
        }
        ShowHealthBar = bl_GameData.Instance.ShowTeamMateHealthBar;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        this.myTransform = this.transform;
        bl_EventHandler.OnRoundEnd += OnGameFinish;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnRoundEnd -= OnGameFinish;
    }

    public void SetName(string DrawName)
    {
        m_PlayerName = DrawName;
#if !UNITY_WEBGL && PVOICE
        NameSize = m_Skin.CalcSize(new GUIContent(m_PlayerName)).x;
        RightPand = (NameSize / 2) + 2;
#endif
    }

    void OnGameFinish()
    {
        isFinish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        if (bl_GameManager.Instance.CameraRendered == null || isFinish)
            return;

        Vector3 vector = bl_GameManager.Instance.CameraRendered.WorldToScreenPoint(m_Target.position);
        if (vector.z > 0)
        {
            int vertical = ShowHealthBar ? 15 : 10;
            if (this.m_dist < m_HideDistance)
            {
                GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - vertical, 10, 11), m_PlayerName, m_Skin);
                if (ShowHealthBar)
                {
                    float mh = (isBot) ? 100 : DamagerManager.maxHealth;
                    float h = (isBot) ? AIHealth.Health : DamagerManager.health;
                    GUI.color = HealthBackColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), mh, HealthBarHeight), BarTexture);
                    GUI.color = HealthBarColor;
                    GUI.DrawTexture(new Rect(vector.x - (mh / 2), (Screen.height - vector.y), h, HealthBarHeight), BarTexture);
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.DrawTexture(new Rect(vector.x - (LargeIconSize / 2), (Screen.height - vector.y) - (LargeIconSize / 2), LargeIconSize, LargeIconSize), m_HideTexture);
            }
#if !UNITY_WEBGL && PVOICE
            //voice chat icon
            if (!isBot && PRecorder.IsSpeaking)
            {
                GUI.DrawTexture(new Rect(vector.x + RightPand, (Screen.height - vector.y) - vertical, 14, 14), TalkingIcon);
            }
#endif
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (bl_GameManager.Instance.CameraRendered == null)
            return;

        m_dist = bl_UtilityHelper.GetDistance(myTransform.position, bl_GameManager.Instance.CameraRendered.transform.position);
    }
}