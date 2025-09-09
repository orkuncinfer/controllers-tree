using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public sealed class MainMenuPanel : BaseUIPanel
    {
        [SerializeField] private Button _startButton;
        
        public event System.Action OnStartClicked;
        
        protected override void Awake()
        {
            base.Awake();
            _startButton.onClick.AddListener(() =>
            {
                OnStartClicked?.Invoke();
                gameObject.SetActive(false);
            });
        }
    }
}