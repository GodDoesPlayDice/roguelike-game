using TMPro;
using UnityEngine;

namespace UI
{
    public class HUDController : MonoBehaviour
    {
        private GameObject _player;
        private TargetController _playerTargetController;

        public TextMeshProUGUI healthTMP;
        public TextMeshProUGUI damageTMP;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _player.TryGetComponent<TargetController>(out _playerTargetController);
        }


        private void Start()
        {
            // subscribe to player events
            _playerTargetController.events.onHealthChangeEvent.AddListener(OnPlayerHealthChanged);
        }

        private void OnPlayerHealthChanged(TargetController.OnHealthChangeEventArgs onHealthChangeEventArgs)
        {
            healthTMP.text = $"Health: {onHealthChangeEventArgs.currentHealth}";
        }

        private void OnPlayerDamageChanged(float newVal)
        {
            if (_player == null || damageTMP == null) return;
            damageTMP.text = $"Damage: {newVal}";
        }
    }
}