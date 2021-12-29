using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Target;

public class HUDController : MonoBehaviour
{
    public GameObject healthText;
    public GameObject damageText;

    private GameObject _player;
    private TargetController _playerTargetController;

    private TextMeshProUGUI _healthTMP;
    private TextMeshProUGUI _damageTMP;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _player.TryGetComponent<TargetController>(out _playerTargetController);
        healthText.TryGetComponent<TextMeshProUGUI>(out _healthTMP);
        damageText.TryGetComponent<TextMeshProUGUI>(out _damageTMP);
    }


    private void Start()
    {
        UpdateText(_healthTMP, "Health: " + _playerTargetController.Health);
        // subscribe to player events
        _playerTargetController.events.onHealthChangeEvent.AddListener(OnPlayerHealthChanged);
    }

    public void OnPlayerHealthChanged(TargetController.OnHealthChangeEventArgs onHealthChangeEventArgs)
    {
        if (_healthTMP == null || _playerTargetController == null) return;
        UpdateText(_healthTMP, "Health: " + onHealthChangeEventArgs.currentHealth);
    }

    public void OnPlayerDamageChanged(float newVal)
    {
        if (_damageTMP == null || _playerTargetController == null) return;
        UpdateText(_damageTMP, "Damage: " + newVal);
    }

    private void UpdateText(TextMeshProUGUI TextMeshProUGUI, string newValue)
    { 
        TextMeshProUGUI.text = newValue;
    }
}
