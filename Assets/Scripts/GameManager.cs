using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private GameObject _player;
    private TargetController _playerTarget;

    private void Awake()
    {
        _player = GameObject.FindGameObjectsWithTag("Player")[0];
        _player.TryGetComponent<TargetController>(out _playerTarget);
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
        // subscribe to player death event
        if (_playerTarget != null) _playerTarget.events.onDeathEvent.AddListener(OnPlayerDeath);

#if UNITY_ANDROID
        Screen.fullScreen = false;
#endif
    }


    public void OnPlayerDeath(TargetController.OnDeathEventArgs onDeathEventArgs)
    {
        //Time.timeScale = 0;

        // unsubscribe to player death event
        if (_playerTarget != null) _playerTarget.events.onDeathEvent.RemoveListener(OnPlayerDeath);
    }
}
