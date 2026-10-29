using System;
using UnityEngine;

/// <summary>
/// The GameState FSM enum
/// </summary>
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

    /// <summary>
    /// Listen to the state of the game being changed.
    /// Returns the previous and new state, in that order
    /// </summary>
    public event Action<GameState, GameState> StateChanged;

    /// <summary>
    /// Returns the current game state (get only)
    /// </summary>
    public GameState Current { get; private set; } = GameState.Playing;

    private void Awake()
    {
        Instance = this;
        EnterState(Current);
    }

    private void Update()
    {
    }

    /// <summary>
    /// Sets the gamestate
    /// </summary>
    /// <param name="next">The gamestate to set</param>
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

            case GameState.MainMenu:
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }
}