using System;
using UnityEngine;
using System.Collections;

namespace Combat
{
    public class ProjectileController : MonoBehaviour
    {
        public int projectileLiveTime = 2;

        public void StartLiveTime()
        {
            StartCoroutine(ProjectileLiveTimeCoroutine());
        }

        IEnumerator ProjectileLiveTimeCoroutine()
        {
            for (int i = projectileLiveTime; i >= 0; i--)
            {
                if (i == 0) Destroy(gameObject);
                yield return new WaitForSeconds(1);
            }
        }
    }
}