using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Target;

public class GameManager : MonoBehaviour
{

    private GameObject player;
    TargetController playerTarget;

    private void Awake()
    {
        // make the object available between scenes
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameManager");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];

        player.TryGetComponent<TargetController>(out playerTarget);
        // subscribe to player death event
        if (playerTarget != null) playerTarget.events.onDeathEvent.AddListener(OnPlayerDeath);
    }

    public void OnPlayerDeath(TargetController.OnDeathEventArgs onDeathEventArgs)
    {
        //Time.timeScale = 0;

        // unsubscribe to player death event
        if (playerTarget != null) playerTarget.events.onDeathEvent.RemoveListener(OnPlayerDeath);
    }
}
