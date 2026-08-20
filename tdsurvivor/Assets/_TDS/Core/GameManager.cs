using System;
using _Toolkit.Updater;
using UnityEngine;

namespace _TDS.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private UpdateRunner runner;

        public GameState State { get; private set; } = GameState.Playing;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;

            GameState prev = State;
            State = newState;

            switch (newState)
            {
                case GameState.Playing:
                    if (runner != null) runner.IsPaused = false;
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    if (runner != null) runner.IsPaused = true;
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    if (runner != null) runner.IsPaused = true;
                    Time.timeScale = 0f;
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }

        public void Pause() => SetState(GameState.Paused);
        public void Resume() => SetState(GameState.Playing);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
