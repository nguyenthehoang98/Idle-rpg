using _TDS.Core;
using _TDS.GameplayScene.Unit;
using TMPro;
using UnityEngine;

namespace _TDS.GameplayScene.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI txtBaseHp;
        [SerializeField] private TextMeshProUGUI txtWave;
        [SerializeField] private TextMeshProUGUI txtAliveMonsters;

        [Header("Overlay")]
        [SerializeField] private GameObject panelGameOver;
        [SerializeField] private GameObject panelVictory;
        [SerializeField] private TextMeshProUGUI txtGameOverMessage;
        [SerializeField] private TextMeshProUGUI txtVictoryMessage;

        private WaveManager waveManager;

        private void Awake()
        {
            if (panelGameOver != null) panelGameOver.SetActive(false);
            if (panelVictory != null) panelVictory.SetActive(false);
        }

        public void Initialize(WaveManager waveManager)
        {
            this.waveManager = waveManager;

            if (BaseCore.Instance != null)
            {
                BaseCore.Instance.OnHpChanged += UpdateBaseHp;
                UpdateBaseHp(BaseCore.Instance.CurrentHp, BaseCore.Instance.MaxHp);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveCompleted += UpdateWave;
                waveManager.OnAllWavesCompleted += ShowVictory;
                UpdateWave(waveManager.CurrentWave);
            }
        }

        private void Update()
        {
            if (waveManager != null && txtAliveMonsters != null)
            {
                txtAliveMonsters.text = $"Alive: {waveManager.AliveMonsters}";
            }
        }

        private void UpdateBaseHp(int current, int max)
        {
            if (txtBaseHp != null)
            {
                txtBaseHp.text = $"Base HP: {current}/{max}";
            }
        }

        private void UpdateWave(int wave)
        {
            if (txtWave != null && waveManager != null)
            {
                txtWave.text = $"Wave: {wave}/{waveManager.TotalWaves}";
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                case GameState.Victory:
                    ShowVictory();
                    break;
            }
        }

        private void ShowGameOver()
        {
            if (panelGameOver != null)
            {
                panelGameOver.SetActive(true);
            }
            if (txtGameOverMessage != null)
            {
                txtGameOverMessage.text = "GAME OVER";
            }
        }

        private void ShowVictory()
        {
            if (panelVictory != null)
            {
                panelVictory.SetActive(true);
            }
            if (txtVictoryMessage != null)
            {
                txtVictoryMessage.text = "VICTORY!";
            }
        }

        private void OnDestroy()
        {
            if (BaseCore.Instance != null)
            {
                BaseCore.Instance.OnHpChanged -= UpdateBaseHp;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveCompleted -= UpdateWave;
                waveManager.OnAllWavesCompleted -= ShowVictory;
            }
        }
    }
}
