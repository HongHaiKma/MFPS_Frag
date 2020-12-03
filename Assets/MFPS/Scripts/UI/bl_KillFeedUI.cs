using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_KillFeedUI : MonoBehaviour
{
    [SerializeField]private Text KillerText;
    [SerializeField]private Text KilledText;
    [SerializeField]private Text WeaponText;
    [SerializeField]private Image KillTypeImage;
    private CanvasGroup Alpha;

    public void Init(KillFeed feed)
    {
        KillerText.text = feed.Killer;
        KillerText.color = feed.KillerColor;
        KilledText.color = feed.KilledColor;
        KilledText.text = feed.Killed;
        WeaponText.text = string.Format("{0}", feed.HowKill);
        KillTypeImage.gameObject.SetActive(feed.HeatShot);
        Alpha = GetComponent<CanvasGroup>();
        StartCoroutine(Hide(10));
    }

    IEnumerator Hide(float time)
    {
        yield return new WaitForSeconds(time);
        while(Alpha.alpha > 0)
        {
            Alpha.alpha -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

}