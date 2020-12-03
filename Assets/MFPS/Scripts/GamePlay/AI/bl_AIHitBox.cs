using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AIHitBox : MonoBehaviour
{

    public bl_AIShooterHealth AI;
    public Collider m_Collider;
    public bool isHead = false;

    public void DoDamage(int damage, string wn, Vector3 direction, int viewID, bool fromBot, Team team)
    {
        if (AI == null)
            return;

        if (isHead) { damage = 100; }
        AI.DoDamage(damage, wn, direction, viewID, fromBot, team, isHead);
    }
}