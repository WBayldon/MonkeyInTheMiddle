using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class MainMenuView : MonoBehaviour
{
    // Remains after scene change, so they don't bring the title screen back
    private static bool shownThisSession;

    [SerializeField] private bool onlyOncePerSession = true;
    [SerializeField] private string containerName = "Root";

    private const int FadeMs = 250;
    private const int FlashMs = 150;

    private GameStateMachine machine;
    private VisualElement docRoot, container;
    private Label instructions;
    private bool open, closing;

    private void OnEnable()
    {
        machine = GameStateMachine.Instance;
        if (machine == null)
        {
            Debug.LogError("MainMenu needs a GameStateMachine in the scene.", this);
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

        if (onlyOncePerSession && shownThisSession)
        {
            docRoot.style.display = DisplayStyle.None;
            return;
        }

        container.style.transitionProperty = new List<StylePropertyName> { "opacity" };
        container.style.transitionDuration = new List<TimeValue> { new TimeValue(FadeMs, TimeUnit.Millisecond) };
        container.style.opacity = 1f;

        shownThisSession = true;
        open = true;
        closing = false;
        docRoot.style.display = DisplayStyle.Flex;
        machine.SetState(GameState.MainMenu);

        instructions = container.Q<Label>("Instructions");
        if (instructions != null)
        {
            instructions.style.color = StyleKeyword.Null;
            instructions.style.transitionProperty = new List<StylePropertyName> { "color" };
            instructions.style.transitionDuration = new List<TimeValue> { new TimeValue(60, TimeUnit.Millisecond) };
        }
    }

    private void Update()
    {
        if (!open || closing) return;

        if (AnyInputPressed())
            CloseMenu();
    }

    private static bool AnyInputPressed()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;

        var mouse = Mouse.current;
        if (mouse != null && (mouse.leftButton.wasPressedThisFrame ||
                              mouse.rightButton.wasPressedThisFrame)) return true;

        var pad = Gamepad.current;
        if (pad != null && (pad.buttonSouth.wasPressedThisFrame ||
                            pad.startButton.wasPressedThisFrame)) return true;

        return false;
    }

    private void CloseMenu()
    {
        closing = true;

        if (instructions != null)
            instructions.style.color = Color.white;

        container.schedule.Execute(() => container.style.opacity = 0f).ExecuteLater(FlashMs);

        container.schedule.Execute(() =>
        {
            docRoot.style.display = DisplayStyle.None;
            open = false;
            closing = false;
            machine.SetState(GameState.Playing);
        }).ExecuteLater(FlashMs + FadeMs);
    }
}