using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AICovertPointManager : MonoBehaviour
{
    public float MaxDistance = 50;
    public bool ShowGizmos = true;

    public static List<bl_AICoverPoint> AllCovers = new List<bl_AICoverPoint>();

    public static void Register(bl_AICoverPoint co)
    {
        AllCovers.Add(co);
    }

    private void OnDestroy()
    {
        AllCovers.Clear();
    }

    public bl_AICoverPoint GetCloseCover(Transform target)
    {
        bl_AICoverPoint cover = null;
        float d = MaxDistance;
        for(int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.position, AllCovers[i].transform.position);
            if(dis < MaxDistance && dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }

        return cover;
    }

    public bl_AICoverPoint GetCoverOnRadius(Transform target, float radius)
    {
        List<bl_AICoverPoint> list = new List<bl_AICoverPoint>();
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.position, AllCovers[i].transform.position);
            if (dis <= radius)
            {
                list.Add(AllCovers[i]);
            }
        }
        bl_AICoverPoint cp = null;
        if (list.Count > 0)
        {
            cp = list[Random.Range(0, list.Count)];
        }
        if(cp == null) { cp = AllCovers[Random.Range(0, AllCovers.Count)]; }

        return cp;
    }

    public bl_AICoverPoint GetCloseCoverForced(Transform target)
    {
        bl_AICoverPoint cover = null;
        float d = 100000;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.position, AllCovers[i].transform.position);
            if (dis < d)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }

        return cover;
    }

    public bl_AICoverPoint GetCloseCover(Transform target, bl_AICoverPoint overrdidePoint)
    {
        bl_AICoverPoint cover = null;
        float d = MaxDistance;
        for (int i = 0; i < AllCovers.Count; i++)
        {
            float dis = bl_UtilityHelper.Distance(target.position, AllCovers[i].transform.position);
            if (dis < MaxDistance && dis < d && AllCovers[i] != overrdidePoint)
            {
                d = dis;
                cover = AllCovers[i];
            }
        }

        return cover;
    }
}