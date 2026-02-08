using System;
using System.Collections;
using Il2CppTMPro;
using MelonLoader;
using SimpleCall.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SimpleCall.Views;

/// <summary>
/// View: UI for the "Call Dealer" button in the Dealer Management phone app.
/// </summary>
public static class CallDealerButtonView
{
    private const int MAX_INITIALIZATION_ATTEMPTS = 120;
    private const float INITIALIZATION_RETRY_DELAY = 0.5f;

    private const string BUTTON_NAME = "CallDealerButton";
    private const string BUTTON_TEXT = "Call Dealer";
    private const int BUTTON_WIDTH = 150;
    private const int BUTTON_HEIGHT = 40;
    private const int BUTTON_FONT_SIZE = 24;

    private const string BASE_PATH = "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas/DealerManagement/Container/Background";

    private static readonly PathConfig[] UI_PATHS =
    {
        new($"{BASE_PATH}/Content/Container/Details", new Vector3(190, 17, 0)),
        new($"{BASE_PATH}/ScrollingContent/Content/Container/Details", new Vector3(190, 387, 0))
    };

    private static GameObject _targetParent;
    private static GameObject _callDealerButton;
    private static PathConfig _currentPathConfig;

    private record PathConfig(string ParentPath, Vector3 ButtonPosition);

    private static void DebugLog(string message)
    {
        if (ModSettings.EnableLogging.Value)
            MelonLogger.Msg(message);
    }

    public static void Initialize()
    {
        MelonCoroutines.Start(InitializeCoroutine());
    }

    public static void Terminate()
    {
        RemoveButton();
        ResetState();
        DebugLog("[SimpleCall] CallDealerButtonView terminated");
    }

    private static IEnumerator InitializeCoroutine()
    {
        DebugLog("[SimpleCall] Starting button initialization...");

        for (int attempts = 0; attempts < MAX_INITIALIZATION_ATTEMPTS; attempts++)
        {
            if (TryFindUiElements())
            {
                CreateCallDealerButton();
                yield break;
            }

            yield return new WaitForSeconds(INITIALIZATION_RETRY_DELAY);
        }

        MelonLogger.Error($"SimpleCall: Failed to find UI elements after {MAX_INITIALIZATION_ATTEMPTS} attempts");
    }

    private static bool TryFindUiElements()
    {
        foreach (var pathConfig in UI_PATHS)
        {
            var parent = GameObject.Find(pathConfig.ParentPath);
            if (parent != null)
            {
                _targetParent = parent;
                _currentPathConfig = pathConfig;
                DebugLog("[SimpleCall] Found UI");
                return true;
            }
        }

        return false;
    }

    private static void CreateCallDealerButton()
    {
        try
        {
            if (_targetParent == null)
            {
                MelonLogger.Error("SimpleCall: Required UI elements are null");
                return;
            }

            RemoveButton();

            _callDealerButton = new GameObject(BUTTON_NAME) { layer = _targetParent.layer };
            _callDealerButton.transform.SetParent(_targetParent.transform, false);

            SetupButtonComponents(_callDealerButton);
            MelonLogger.Msg("[SimpleCall] Call Dealer button created successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"SimpleCall: Error creating button: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void SetupButtonComponents(GameObject buttonGameObject)
    {
        var rectTransform = buttonGameObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = _currentPathConfig.ButtonPosition;

        buttonGameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

        SetupButtonText(buttonGameObject);

        var button = buttonGameObject.AddComponent<Button>();
        button.onClick.AddListener(new Action(OnCallDealerButtonClicked));
    }

    private static void SetupButtonText(GameObject buttonGameObject)
    {
        var textObject = new GameObject("ButtonText");
        textObject.transform.SetParent(buttonGameObject.transform, false);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        var textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = BUTTON_TEXT;
        textComponent.color = Color.white;
        textComponent.fontSize = BUTTON_FONT_SIZE;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = false;
        textComponent.overflowMode = TextOverflowModes.Overflow;
    }

    private static void OnCallDealerButtonClicked()
    {
        DealerCallController.CallDealerToPlayer();
    }

    private static void RemoveButton()
    {
        if (_callDealerButton != null)
        {
            Object.Destroy(_callDealerButton);
            _callDealerButton = null;
        }
    }

    private static void ResetState()
    {
        _targetParent = null;
        _currentPathConfig = null;
        _callDealerButton = null;
    }
}
