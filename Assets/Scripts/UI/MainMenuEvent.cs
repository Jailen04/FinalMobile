
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuEvent : MonoBehaviour
{
    private UIDocument _document;

    private Button _startButton;
    private Button _quitButton;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        _startButton = _document.rootVisualElement.Q<Button>("StartGameButton");
        _quitButton = _document.rootVisualElement.Q<Button>("QuitGameButton");

        _startButton.clicked += OnPlayGameClick;
        _quitButton.clicked += OnQuitGameClick;
    }

    private void OnDisable()
    {
        if (_startButton != null)
            _startButton.clicked -= OnPlayGameClick;

        if (_quitButton != null)
            _quitButton.clicked -= OnQuitGameClick;
    }

    private void OnPlayGameClick()
    {
        Debug.Log("You Pressed the Start Button!");

        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitGameClick()
    {
        Debug.Log("You Pressed the Quit Button!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
