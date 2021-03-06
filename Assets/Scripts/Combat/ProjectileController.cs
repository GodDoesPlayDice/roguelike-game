using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Combat
{
    public class ProjectileController : MonoBehaviour
    {
        public int projectileLiveTime = 2;
        public float damage = 0;
        public UnityEvent<GameObject> onProjectileHitEvent;

        public List<string> ignoreTags = new List<string>();
        private void Awake()
        {
            onProjectileHitEvent ??= new UnityEvent<GameObject>();
            Debug.Log(gameObject.name);
        }

        private void OnTriggerEnter(Collider other)
        {
            onProjectileHitEvent.Invoke(other.gameObject);

            if (!ignoreTags.Contains(other.gameObject.tag))
            { 
                other.gameObject.TryGetComponent<TargetController>(out var targetController);
                if (targetController != null) targetController.TakeDamage(damage);
                Destroy(gameObject);
            };
            
        }

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