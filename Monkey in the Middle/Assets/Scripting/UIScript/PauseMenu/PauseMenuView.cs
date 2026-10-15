using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuView : MonoBehaviour
{
    public event Action UnstuckRequested;
    public event Action LevelSelectRequested;

    private VisualElement root;
    private GameStateMachine machine;

    private Button resumeButton, unstuckButton, levelSelectButton, quitButton;

    private void OnEnable()
    {
        machine = GameStateMachine.Instance;

        var docRoot = GetComponent<UIDocument>().rootVisualElement;
        docRoot.style.flexGrow = 1;
        root = docRoot.Q<VisualElement>("Root");

        resumeButton = root.Q<Button>("ResumeButton");
        unstuckButton = root.Q<Button>("UnstuckButton");
        levelSelectButton = root.Q<Button>("LevelSelectButton");
        quitButton = root.Q<Button>("QuitGameButton");

        resumeButton.clicked += OnResumeClicked;
        unstuckButton.clicked += OnUnstuckClicked;
        levelSelectButton.clicked += OnLevelSelectClicked;
        quitButton.clicked += OnQuitClicked;

        machine.StateChanged += OnStateChanged;
        Apply(machine.Current);
    }

    private void OnDisable()
    {
        if (resumeButton != null)
        {
            resumeButton.clicked -= OnResumeClicked;
            unstuckButton.clicked -= OnUnstuckClicked;
            levelSelectButton.clicked -= OnLevelSelectClicked;
            quitButton.clicked -= OnQuitClicked;
        }

        if (machine != null) machine.StateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        switch (machine.Current)
        {
            case GameState.Playing: machine.SetState(GameState.Paused); break;
            case GameState.Paused: machine.SetState(GameState.Playing); break;
        }
    }

    private void OnResumeClicked() => machine.SetState(GameState.Playing);
    private void OnUnstuckClicked() => UnstuckRequested?.Invoke();
    private void OnLevelSelectClicked() => LevelSelectRequested?.Invoke();
    private void OnQuitClicked() => Application.Quit();

    private void OnStateChanged(GameState previous, GameState next) => Apply(next);

    private void Apply(GameState state)
    {
        root.style.display = state == GameState.Paused ? DisplayStyle.Flex : DisplayStyle.None;
    }
}