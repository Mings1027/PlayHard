using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button[] animatedButtons;

        [SerializeField] private float bounceScale = 1.2f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject titleUIPanel;
        [SerializeField] private GameObject gamePlayPanel;

        private Sequence _playButtonSequence;

        private void Awake()
        {
            InitUI();
            InitButtonAnimations();
            InitButtonEvents();
        }

        private void OnEnable()
        {
            UIEventManager.AddEvent(UIEvent.PlayGame, PlayGame);
        }

        private void OnDisable()
        {
            UIEventManager.RemoveEvent(UIEvent.PlayGame, PlayGame);
        }


        private void InitUI()
        {
            titleUIPanel.SetActive(true);
            gamePlayPanel.SetActive(false);
        }

        private void InitButtonAnimations()
        {
            for (int i = 0; i < animatedButtons.Length; i++)
            {
                if (!animatedButtons[i].TryGetComponent(out ButtonAnimator _))
                {
                    animatedButtons[i].gameObject.AddComponent<ButtonAnimator>();
                }
            }
        }

        private void InitButtonEvents()
        {
            playButton.onClick.AddListener(() =>
            {
                UIEventManager.TriggerEvent(UIEvent.PlayGame);
                UIEventManager.TriggerEvent(UIEvent.StartStage);
            });
        }

        private void PlayGame()
        {
            titleUIPanel.SetActive(false);
            gamePlayPanel.SetActive(true);
            
        }

        private void GoHome()
        {
            gamePlayPanel.SetActive(false);
            titleUIPanel.SetActive(true);
        }
    }
}