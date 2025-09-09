using Game.Services;
using Game.Views.UI;
using UnityEngine;

namespace Game.Infrastructure
{
    public sealed class UIBootstrapper : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private MainMenuPanel _mainMenuPanel;
        [SerializeField] private WinPanel _winPanel;
        [SerializeField] private LosePanel _losePanel;
        [SerializeField] private PausePanel _pausePanel; // NEW!
        
        private IUIService _uiService;
        
        private void Awake()
        {
            // Hide all panels at start
            if (_mainMenuPanel) _mainMenuPanel.gameObject.SetActive(false);
            if (_winPanel) _winPanel.gameObject.SetActive(false);
            if (_losePanel) _losePanel.gameObject.SetActive(false);
            if (_pausePanel) _pausePanel.gameObject.SetActive(false);
        }
        
        public void Initialize(IUIService uiService)
        {
            _uiService = uiService;
            
            // Register all panels
            if (_mainMenuPanel) _uiService.RegisterPanel("MainMenu", _mainMenuPanel);
            if (_winPanel) _uiService.RegisterPanel("WinPanel", _winPanel);
            if (_losePanel) _uiService.RegisterPanel("LosePanel", _losePanel);
            if (_pausePanel) _uiService.RegisterPanel("PausePanel", _pausePanel);
        }
        
        private void OnDestroy()
        {
            // Unregister panels
            if (_uiService != null)
            {
                _uiService.UnregisterPanel("MainMenu");
                _uiService.UnregisterPanel("WinPanel");
                _uiService.UnregisterPanel("LosePanel");
                _uiService.UnregisterPanel("PausePanel");
            }
        }
    }
}