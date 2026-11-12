using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuView : MonoBehaviour
{
    private VisualElement root;
    private GameStateMachine machine;
    private VisualElement background, mainPausePanel, levelSelectPanel, winPanel;
    private VisualElement shown;
    private bool animating;
    private bool levelSelectFromWin;
    private IVisualElementScheduledItem swapTick;

    private Button resumeButton, unstuckButton, levelSelectButton, mainMenuButton, backButton;
    private Button winRestartButton, winNextLevelButton, winLevelSelectButton, winMainMenuButton;

    private readonly List<(Button button, System.Action handler)> levelBindings = new();

    private const int FadeMs = 250;
    private const int SwapMs = 900;
    private const float PeekPercent = 100f;
    private static readonly Vector2 BasePercent = new Vector2(0, 0);
    private static readonly Translate RestTranslate = new Translate(new Length(BasePercent.x, LengthUnit.Percent), new Length(BasePercent.y, LengthUnit.Percent));
    private static readonly Scale RestScale = new Scale(new Vector3(1f, 1f, 1f));

    [SerializeField] private SyncLevels levels;
    [SerializeField] private MainMenuView mainMenu;


    private void OnEnable()
    {
        machine = GameStateMachine.Instance;
        if (machine == null)
        {
            Debug.LogError("PauseMenu needs a GameStateMachine in the scene.", this);
            enabled = false;
            return;
        }

        if (levels == null)
        {
            Debug.LogError("PauseMenu needs a LevelList assigned.", this);
            enabled = false;
            return;
        }

        var docRoot = GetComponent<UIDocument>().rootVisualElement;
        docRoot.style.flexGrow = 1;
        root = docRoot.Q<VisualElement>("Root");

        background = root.Q<VisualElement>("Background");

        mainPausePanel = root.Q<VisualElement>("MainPause");
        levelSelectPanel = root.Q<VisualElement>("LevelSelect");
        winPanel = root.Q<VisualElement>("Win");

        resumeButton = mainPausePanel.Q<Button>("ResumeButton");
        unstuckButton = mainPausePanel.Q<Button>("UnstuckButton");
        levelSelectButton = mainPausePanel.Q<Button>("LevelSelectButton");
        mainMenuButton = mainPausePanel.Q<Button>("MainMenuButton");

        backButton = levelSelectPanel.Q<Button>("BackButton");

        winRestartButton = winPanel.Q<Button>("UnstuckButton");
        winNextLevelButton = winPanel.Q<Button>("NextLevelButton");
        winLevelSelectButton = winPanel.Q<Button>("LevelSelectButton");
        winMainMenuButton = winPanel.Q<Button>("MainMenuButton");

        winNextLevelButton.SetEnabled(GetNextLevelName() != null);

        resumeButton.clicked += OnResumeClicked;
        unstuckButton.clicked += OnUnstuckClicked;
        levelSelectButton.clicked += OnLevelSelectClicked;
        mainMenuButton.clicked += OnMainMenuClicked;
        for (int i = 0; i < levels.Count; i++)
        {
            var button = levelSelectPanel.Q<Button>($"Level{i + 1}Button");
            if (button == null) continue;

            string scene = levels.Get(i);
            System.Action handler = () => { if (!animating) LoadLevel(scene); };
            button.clicked += handler;
            levelBindings.Add((button, handler));
        }
        backButton.clicked += OnBackClicked;

        winRestartButton.clicked += OnWinRestartClicked;
        winNextLevelButton.clicked += OnWinNextLevelClicked;
        winLevelSelectButton.clicked += OnWinLevelSelectClicked;
        winMainMenuButton.clicked += OnMainMenuClicked;

        // Renders the pause menu invisibly to prevent lag when opening it the first time
        SetAnimating(true);
        root.AddToClassList("closed");
        background.style.display = DisplayStyle.Flex;
        mainPausePanel.style.display = DisplayStyle.Flex;
        levelSelectPanel.style.display = DisplayStyle.Flex;
        winPanel.style.display = DisplayStyle.Flex;
        root.schedule.Execute(HideAll).ExecuteLater(100);
    }

    private void OnDisable()
    {
        swapTick?.Pause();

        foreach (var (button, handler) in levelBindings)
            button.clicked -= handler;
        levelBindings.Clear();

        if (resumeButton != null)
        {
            resumeButton.clicked -= OnResumeClicked;
            unstuckButton.clicked -= OnUnstuckClicked;
            levelSelectButton.clicked -= OnLevelSelectClicked;
            mainMenuButton.clicked -= OnMainMenuClicked;

            backButton.clicked -= OnBackClicked;

            winRestartButton.clicked -= OnWinRestartClicked;
            winNextLevelButton.clicked -= OnWinNextLevelClicked;
            winLevelSelectButton.clicked -= OnWinLevelSelectClicked;
            winMainMenuButton.clicked -= OnMainMenuClicked;
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
                OpenMenu(mainPausePanel);
                break;

            case GameState.Paused:
                if (shown == winPanel)
                    break;
                else if (shown == levelSelectPanel)
                    ShowPanel(levelSelectFromWin ? winPanel : mainPausePanel, levelSelectPanel);
                else
                    CloseMenu();
                break;
        }
    }

    // Main Pause
    private void OnResumeClicked() { if (!animating) CloseMenu(); }
    private void OnUnstuckClicked() { if (!animating) Unstuck(machine.Current); }
    private void OnLevelSelectClicked() { if (!animating) { levelSelectFromWin = false; ShowPanel(levelSelectPanel, mainPausePanel); } }
    private void OnMainMenuClicked() { OpenMainMenu(); }

    // Level Select
    private void OnBackClicked()
    {
        if (animating) return;
        ShowPanel(levelSelectFromWin ? winPanel : mainPausePanel, levelSelectPanel);
    }

    // Win
    private void OnWinRestartClicked() { if (!animating) Unstuck(machine.Current); }
    private void OnWinNextLevelClicked()
    {
        if (animating) return;

        string next = GetNextLevelName();
        if (next != null) LoadLevel(next);
    }
    private void OnWinLevelSelectClicked()
    {
        if (animating) return;
        levelSelectFromWin = true;
        ShowPanel(levelSelectPanel, winPanel);
    }

    private void OpenMenu(VisualElement panel)
    {
        machine.SetState(GameState.Paused);

        SetAnimating(true);
        root.AddToClassList("closed");
        levelSelectFromWin = false;
        ShowPanel(panel);

        root.schedule.Execute(() => root.RemoveFromClassList("closed")).ExecuteLater(30);
        root.schedule.Execute(() => SetAnimating(false)).ExecuteLater(30 + FadeMs);
    }

    private void OpenMainMenu()
    {
        if (animating) return;
        CloseMenu(() => mainMenu.Show());
    }

    private void CloseMenu(System.Action onClosed = null)
    {
        SetAnimating(true);
        root.AddToClassList("closed");

        root.schedule.Execute(() =>
        {
            HideAll();

            if (onClosed != null)
                onClosed();
            else
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

        ApplySwap(newPanel, 0f, BasePercent);

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

            ApplySwap(newPanel, p, BasePercent);

            if (p < 1f) return;

            tick.Pause();
            ClearShuffle();
            FadeOutAndHide(oldPanel);
        }).Every(10);

        swapTick = tick;
    }

    private static void ApplySwap(VisualElement panel, float p, Vector2 basePct)
    {
        float e = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * p);
        float x = PeekPercent * Mathf.Sin(Mathf.PI * e);
        float s = 0.95f + 0.05f * e + 0.075f * Mathf.Sin(Mathf.PI * e);

        panel.style.translate = new Translate(
            new Length(basePct.x + x, LengthUnit.Percent),
            new Length(basePct.y, LengthUnit.Percent));
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
        winPanel.style.display = DisplayStyle.None;
        background.style.display = DisplayStyle.None;
    }

    private void ClearShuffle()
    {
        foreach (var p in new[] { mainPausePanel, levelSelectPanel, winPanel })
        {
            p.style.translate = RestTranslate;
            p.style.scale = RestScale;
            p.style.opacity = StyleKeyword.Null;
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

    private string GetNextLevelName()
    {
        string next = levels.GetNext(SceneManager.GetActiveScene().name);

        if (string.IsNullOrEmpty(next) || !Application.CanStreamedLevelBeLoaded(next))
            return null;

        return next;
    }

    private void FadeOutAndHide(VisualElement panel)
    {
        float start = Time.unscaledTime;

        IVisualElementScheduledItem fade = null;
        fade = root.schedule.Execute(() =>
        {
            float t = Mathf.Clamp01((Time.unscaledTime - start) * 1000f / FadeMs);
            panel.style.opacity = 1f - t;

            if (t < 1f) return;

            fade.Pause();
            panel.style.display = DisplayStyle.None;
            panel.style.opacity = StyleKeyword.Null;
            SetAnimating(false);
        }).Every(10);

        swapTick = fade;
    }

    /// <summary>
    /// Opens the win menu
    /// </summary>
    public void OpenWinMenu()
    {
        SetAnimating(true);
        root.AddToClassList("closed");
        OpenMenu(winPanel);

        root.schedule.Execute(() => root.RemoveFromClassList("closed")).ExecuteLater(30);
        root.schedule.Execute(() => SetAnimating(false)).ExecuteLater(30 + FadeMs);
    }
}