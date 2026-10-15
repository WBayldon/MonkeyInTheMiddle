using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuView : MonoBehaviour
{
    private VisualElement root;
    private GameStateMachine machine;

    private VisualElement background, mainPausePanel, levelSelectPanel;

    private Button resumeButton, unstuckButton, levelSelectButton, quitButton;
    private Button level1Button, level2Button, level3Button, backButton;
    [SerializeField] private string Level1SceneName, Level2SceneName, Level3SceneName;

    private void OnEnable()
    {
        machine = GameStateMachine.Instance;
        if (machine == null)
        {
            Debug.LogError("PauseMenu needs a GameStateMachine in the scene.", this);
            enabled = false;
            return;
        }

        var docRoot = GetComponent<UIDocument>().rootVisualElement;
        docRoot.style.flexGrow = 1;
        root = docRoot.Q<VisualElement>("Root");

        background = root.Q<VisualElement>("Background");

        mainPausePanel = root.Q<VisualElement>("MainPause");
        levelSelectPanel = root.Q<VisualElement>("LevelSelect");

        resumeButton = root.Q<Button>("ResumeButton");
        unstuckButton = root.Q<Button>("UnstuckButton");
        levelSelectButton = root.Q<Button>("LevelSelectButton");
        quitButton = root.Q<Button>("QuitGameButton");

        level1Button = root.Q<Button>("Level1Button");
        level2Button = root.Q<Button>("Level2Button");
        level3Button = root.Q<Button>("Level3Button");
        backButton = root.Q<Button>("BackButton");

        resumeButton.clicked += OnResumeClicked;
        unstuckButton.clicked += OnUnstuckClicked;
        levelSelectButton.clicked += OnLevelSelectClicked;
        quitButton.clicked += OnQuitClicked;

        level1Button.clicked += OnLevel1Clicked;
        level2Button.clicked += OnLevel2Clicked;
        level3Button.clicked += OnLevel3Clicked;
        backButton.clicked += OnBackClicked;
    }

    private void OnDisable()
    {
        if (resumeButton != null)
        {
            resumeButton.clicked -= OnResumeClicked;
            unstuckButton.clicked -= OnUnstuckClicked;
            levelSelectButton.clicked -= OnLevelSelectClicked;
            quitButton.clicked -= OnQuitClicked;

            level1Button.clicked -= OnLevel1Clicked;
            level2Button.clicked -= OnLevel2Clicked;
            level3Button.clicked -= OnLevel3Clicked;
            backButton.clicked -= OnBackClicked;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        switch (machine.Current)
        {
            case GameState.Playing:
                machine.SetState(GameState.Paused);
                ShowPanel(mainPausePanel);
                break;

            case GameState.Paused:
                if (levelSelectPanel.style.display == DisplayStyle.Flex)
                    ShowPanel(mainPausePanel);
                else
                {
                    machine.SetState(GameState.Playing);
                    HideAll();
                }
                break;
        }
    }

    private void OnResumeClicked()
    {
        machine.SetState(GameState.Playing);
        HideAll();
    }
    private void OnUnstuckClicked() => Unstuck(machine.Current);
    private void OnLevelSelectClicked() => OpenLevelSelect();
    private void OnQuitClicked() => Application.Quit();
    private void OnLevel1Clicked() => LoadLevel(Level1SceneName);
    private void OnLevel2Clicked() => LoadLevel(Level2SceneName);
    private void OnLevel3Clicked() => LoadLevel(Level3SceneName);
    private void OnBackClicked() => ShowPanel(mainPausePanel);

    private void ShowPanel(VisualElement panel)
    {
        mainPausePanel.style.display = panel == mainPausePanel ? DisplayStyle.Flex : DisplayStyle.None;
        levelSelectPanel.style.display = panel == levelSelectPanel ? DisplayStyle.Flex : DisplayStyle.None;
        background.style.display = DisplayStyle.Flex;
    }

    private void HideAll()
    {
        mainPausePanel.style.display = DisplayStyle.None;
        levelSelectPanel.style.display = DisplayStyle.None;
        background.style.display = DisplayStyle.None;
    }

    private void Unstuck(GameState state)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void OpenLevelSelect()
    {
        ShowPanel(levelSelectPanel);
    }

    private void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}