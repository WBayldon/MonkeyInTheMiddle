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
    private VisualElement shown;
    private bool animating;
    private IVisualElementScheduledItem swapTick;

    private Button resumeButton, unstuckButton, levelSelectButton, quitButton;
    private Button level1Button, level2Button, level3Button, backButton;

    private const int FadeMs = 250;
    private const int SwapMs = 900;
    private const float PeekPercent = 100f;

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

        // Renders the pause menu invisibly to prevent lag when opening it the first time
        SetAnimating(true);
        root.AddToClassList("closed");
        background.style.display = DisplayStyle.Flex;
        mainPausePanel.style.display = DisplayStyle.Flex;
        levelSelectPanel.style.display = DisplayStyle.Flex;
        root.schedule.Execute(HideAll).ExecuteLater(100);
    }

    private void OnDisable()
    {
        swapTick?.Pause();

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

        if (animating) return;

        switch (machine.Current)
        {
            case GameState.Playing:
                OpenMenu();
                break;

            case GameState.Paused:
                if (shown == levelSelectPanel)
                    ShowPanel(mainPausePanel, levelSelectPanel);
                else
                    CloseMenu();
                break;
        }
    }

    // Main Pause
    private void OnResumeClicked() { if (!animating) CloseMenu(); }
    private void OnUnstuckClicked() { if (!animating) Unstuck(machine.Current); }
    private void OnLevelSelectClicked() { if (!animating) ShowPanel(levelSelectPanel, mainPausePanel); }
    private void OnQuitClicked() { if (!animating) Application.Quit(); }

    // Level Select
    private void OnLevel1Clicked() { if (!animating) LoadLevel(Level1SceneName); }
    private void OnLevel2Clicked() { if (!animating) LoadLevel(Level2SceneName); }
    private void OnLevel3Clicked() { if (!animating) LoadLevel(Level3SceneName); }
    private void OnBackClicked() { if (!animating) ShowPanel(mainPausePanel, levelSelectPanel); }

    private void OpenMenu()
    {
        machine.SetState(GameState.Paused);

        SetAnimating(true);
        root.AddToClassList("closed");
        ShowPanel(mainPausePanel);

        root.schedule.Execute(() => root.RemoveFromClassList("closed")).ExecuteLater(30);
        root.schedule.Execute(() => SetAnimating(false)).ExecuteLater(30 + FadeMs);
    }

    private void CloseMenu()
    {
        SetAnimating(true);
        root.AddToClassList("closed");

        root.schedule.Execute(() =>
        {
            HideAll();
            machine.SetState(GameState.Playing);
        }).ExecuteLater(FadeMs);
    }

    private void ShowPanel(VisualElement newPanel, VisualElement oldPanel = null)
    {
        shown = newPanel;
        background.style.display = DisplayStyle.Flex;

        swapTick?.Pause();
        ClearShuffle();
        newPanel.style.display = DisplayStyle.Flex;

        if (oldPanel == null) return;

        SetAnimating(true);
        oldPanel.BringToFront();
        newPanel.AddToClassList("driven");
        ApplySwap(newPanel, 0f);

        float startTime = Time.unscaledTime;
        bool inFront = false;

        IVisualElementScheduledItem tick = null;
        tick = root.schedule.Execute(() =>
        {
            if (shown != newPanel) { tick.Pause(); return; }

            float p = Mathf.Clamp01((Time.unscaledTime - startTime) * 1000f / SwapMs);

            if (!inFront && p >= 0.5f)
            {
                newPanel.BringToFront();
                inFront = true;
            }

            ApplySwap(newPanel, p);

            if (p < 1f) return;

            tick.Pause();
            ClearShuffle();
            oldPanel.style.display = DisplayStyle.None;
            SetAnimating(false);
        }).Every(10);

        swapTick = tick;
    }

    private static void ApplySwap(VisualElement panel, float p)
    {
        float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * p);
        float x = PeekPercent * Mathf.Sin(Mathf.PI * e);
        float s = 0.95f + 0.05f * e + 0.075f * Mathf.Sin(Mathf.PI * e);

        panel.style.translate = new Translate(new Length(x, LengthUnit.Percent), new Length(0));
        panel.style.scale = new Scale(new Vector3(s, s, 1f));
    }

    private void HideAll()
    {
        shown = null;
        swapTick?.Pause();
        SetAnimating(false);
        ClearShuffle();
        mainPausePanel.style.display = DisplayStyle.None;
        levelSelectPanel.style.display = DisplayStyle.None;
        background.style.display = DisplayStyle.None;
    }

    private void ClearShuffle()
    {
        foreach (var p in new[] { mainPausePanel, levelSelectPanel })
        {
            p.style.translate = StyleKeyword.Null;
            p.style.scale = StyleKeyword.Null;
            p.RemoveFromClassList("driven");
        }
    }

    private void SetAnimating(bool value)
    {
        animating = value;
        if (value) root.AddToClassList("locked");
        else root.RemoveFromClassList("locked");
    }

    private void Unstuck(GameState state)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}