using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuView : MonoBehaviour
{
    // Remains after scene change, so they don't bring the title screen back
    private static bool shownThisSession;

    [SerializeField] private bool onlyOncePerSession = true;
    [SerializeField] private string containerName = "MainMenu";
    [SerializeField] private SyncLevels levels;

    [SerializeField] private UnityEvent onSettings;

    private const int FadeMs = 250;
    private const int SlideMs = 500;

    private GameStateMachine machine;
    private VisualElement docRoot, container;
    private VisualElement leftControls, rightControls, levelSelectPanel;
    private Button newGameButton, levelSelectButton, settingsButton, quitButton, levelSelectBackButton;
    private IVisualElementScheduledItem slideTick;
    private bool closing, sliding, levelSelectOpen;

    private readonly List<(Button button, System.Action handler)> levelBindings = new();

    private void OnEnable()
    {
        machine = GameStateMachine.Instance;
        if (machine == null)
        {
            Debug.LogError("MainMenu needs a GameStateMachine in the scene.", this);
            enabled = false;
            return;
        }

        if (levels == null)
        {
            Debug.LogError("MainMenu needs a LevelList assigned.", this);
            enabled = false;
            return;
        }

        docRoot = GetComponent<UIDocument>().rootVisualElement;
        docRoot.style.position = Position.Absolute;
        docRoot.style.left = 0;
        docRoot.style.right = 0;
        docRoot.style.top = 0;
        docRoot.style.bottom = 0;

        container = docRoot.Q<VisualElement>(containerName);
        if (container == null)
        {
            Debug.LogError($"No element named '{containerName}' in the main menu UXML.", this);
            enabled = false;
            return;
        }

        leftControls = container.Q<VisualElement>("LeftControls");
        rightControls = container.Q<VisualElement>("RightControls");
        levelSelectPanel = container.Q<VisualElement>("LevelSelect");

        newGameButton = leftControls.Q<Button>("NewGameButton");
        levelSelectButton = leftControls.Q<Button>("LevelSelectButton");
        settingsButton = rightControls.Q<Button>("SettingsButton");
        quitButton = rightControls.Q<Button>("QuitGameButton");
        levelSelectBackButton = levelSelectPanel.Q<Button>("BackButton");

        newGameButton.clicked += OnNewGameClicked;
        levelSelectButton.clicked += OnLevelSelectClicked;
        settingsButton.clicked += OnSettingsClicked;
        quitButton.clicked += OnQuitClicked;
        levelSelectBackButton.clicked += OnLevelSelectBackClicked;
        for (int i = 0; i < levels.Count; i++)
        {
            var button = levelSelectPanel.Q<Button>($"Level{i + 1}Button");
            if (button == null) continue;

            string scene = levels.Get(i);
            System.Action handler = () => { if (!sliding && !closing) SceneManager.LoadScene(scene); };
            button.clicked += handler;
            levelBindings.Add((button, handler));
        }

        container.style.transitionProperty = new List<StylePropertyName> { "opacity" };
        container.style.transitionDuration = new List<TimeValue> { new TimeValue(FadeMs, TimeUnit.Millisecond) };

        if (onlyOncePerSession && shownThisSession)
        {
            docRoot.style.display = DisplayStyle.None;
            return;
        }

        shownThisSession = true;
        Show();
    }

    private void OnDisable()
    {
        slideTick?.Pause();

        foreach (var (button, handler) in levelBindings)
            button.clicked -= handler;
        levelBindings.Clear();

        if (newGameButton != null)
        {
            newGameButton.clicked -= OnNewGameClicked;
            levelSelectButton.clicked -= OnLevelSelectClicked;
            settingsButton.clicked -= OnSettingsClicked;
            quitButton.clicked -= OnQuitClicked;
            levelSelectBackButton.clicked -= OnLevelSelectBackClicked;
        }
    }

    /// <summary>
    /// Shows the main menu (call from the pause menu's Main Menu button)
    /// </summary>
    public void Show()
    {
        slideTick?.Pause();

        closing = false;
        sliding = false;
        levelSelectOpen = false;

        levelSelectPanel.style.display = DisplayStyle.None;
        levelSelectPanel.style.translate = StyleKeyword.Null;
        levelSelectPanel.RemoveFromClassList("driven");
        container.RemoveFromClassList("locked");
        leftControls.RemoveFromClassList("locked");

        container.style.opacity = 1f;
        docRoot.style.display = DisplayStyle.Flex;
        machine.SetState(GameState.MainMenu);
    }

    // Buttons
    private void OnNewGameClicked() { if (!closing && !sliding && !levelSelectOpen) SceneManager.LoadScene(levels.GetFirst()); }

    private void OnLevelSelectClicked()
    {
        if (closing || sliding || levelSelectOpen) return;
        SlideLevelSelectIn();
    }

    private void OnSettingsClicked() { if (!closing && !sliding) onSettings?.Invoke(); }

    private void OnQuitClicked()
    {
        if (closing || sliding) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnLevelSelectBackClicked()
    {
        if (closing || sliding || !levelSelectOpen) return;
        SlideLevelSelectOut();
    }

    private void CloseMenu()
    {
        closing = true;
        container.style.opacity = 0f;

        container.schedule.Execute(() =>
        {
            docRoot.style.display = DisplayStyle.None;
            closing = false;
            machine.SetState(GameState.Playing);
        }).ExecuteLater(FadeMs);
    }

    private void SlideLevelSelectIn()
    {
        sliding = true;
        levelSelectOpen = true;
        leftControls.AddToClassList("locked");
        container.AddToClassList("locked");
        levelSelectPanel.AddToClassList("driven");
        levelSelectPanel.style.translate = new Translate(new Length(0), Length.Percent(100));
        levelSelectPanel.style.display = DisplayStyle.Flex;

        float startTime = Time.unscaledTime;

        slideTick = levelSelectPanel.schedule.Execute(() =>
        {
            float t = Mathf.Clamp01((Time.unscaledTime - startTime) * 1000f / SlideMs);
            float e = 1f - Mathf.Pow(1f - t, 3f);   // ease out

            levelSelectPanel.style.translate = new Translate(new Length(0), Length.Percent(100f * (1f - e)));

            if (t < 1f) return;

            slideTick.Pause();
            levelSelectPanel.style.translate = StyleKeyword.Null;
            levelSelectPanel.RemoveFromClassList("driven");
            container.RemoveFromClassList("locked");
            sliding = false;
        }).Every(10);
    }

    private void SlideLevelSelectOut()
    {
        sliding = true;
        container.AddToClassList("locked");
        levelSelectPanel.AddToClassList("driven");

        float startTime = Time.unscaledTime;

        slideTick = levelSelectPanel.schedule.Execute(() =>
        {
            float t = Mathf.Clamp01((Time.unscaledTime - startTime) * 1000f / SlideMs);
            float e = t * t;   // ease in

            levelSelectPanel.style.translate = new Translate(new Length(0), Length.Percent(100f * e));

            if (t < 1f) return;

            slideTick.Pause();
            levelSelectPanel.style.display = DisplayStyle.None;
            levelSelectPanel.style.translate = StyleKeyword.Null;
            levelSelectPanel.RemoveFromClassList("driven");
            leftControls.RemoveFromClassList("locked");
            container.RemoveFromClassList("locked");
            levelSelectOpen = false;
            sliding = false;
        }).Every(10);
    }
}