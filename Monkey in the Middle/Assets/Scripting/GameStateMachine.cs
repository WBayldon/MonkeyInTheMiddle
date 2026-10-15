using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    MainMenu,
    LevelSelect,
    Loading,
    Playing,
    Paused,
}

[DefaultExecutionOrder(-100)]
public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }

    public event Action<GameState, GameState> StateChanged;

    public GameState Current { get; private set; } = GameState.Playing;
    public bool GameplayInputAllowed => Current == GameState.Playing;

    private void Awake()
    {
        Instance = this;
        EnterState(Current);
    }

    private void Update()
    {
    }

    public void SetState(GameState next)
    {
        if (next == Current) return;

        GameState previous = Current;
        Current = next;
        EnterState(next);

        StateChanged?.Invoke(previous, next);
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                AudioListener.pause = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }
}