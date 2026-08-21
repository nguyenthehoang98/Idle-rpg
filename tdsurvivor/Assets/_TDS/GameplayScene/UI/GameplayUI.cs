using _TDS.Core;
using _TDS.GameplayScene.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            // Click UI cần EventSystem - tự tạo nếu scene chưa có
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
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
                EnsureReplayButton(panelGameOver.transform);
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
                EnsureReplayButton(panelVictory.transform);
            }
            if (txtVictoryMessage != null)
            {
                txtVictoryMessage.text = "VICTORY!";
            }
        }

        /// <summary>Tự tạo nút chơi lại nếu panel chưa có - không phụ thuộc setup scene.</summary>
        private void EnsureReplayButton(Transform panel)
        {
            if (panel.Find("BtnReplay") != null) return;

            GameObject go = new GameObject("BtnReplay");
            go.transform.SetParent(panel, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = new Vector2(0, -50);
            rt.sizeDelta = new Vector2(160, 45);

            go.AddComponent<CanvasRenderer>();
            go.AddComponent<Image>().color = new Color(0.25f, 0.55f, 1f);

            Button button = go.AddComponent<Button>();
            button.onClick.AddListener(Restart);

            GameObject label = new GameObject("Text");
            label.transform.SetParent(go.transform, false);
            RectTransform lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;
            label.AddComponent<CanvasRenderer>();
            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "CHƠI LẠI";
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>Gọi từ nút Replay trên GameOver/Victory panel.</summary>
        public void Restart()
        {
            Time.timeScale = 1f; // GameManager đã set 0 khi thua/thắng
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
