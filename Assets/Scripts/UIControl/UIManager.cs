using DataControl;
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

        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private StageData stageData;

        [SerializeField] private BoxCollider2D bubbleContainer;

        private Sequence _playButtonSequence;

        private void Awake()
        {
            InitUI();
            InitButtonAnimations();
            InitButtonEvents();
        }

        private void OnEnable()
        {
            EventManager.AddEvent(ActionEvent.PlayGame, PlayGame);
        }

        private void OnDisable()
        {
            EventManager.RemoveEvent(ActionEvent.PlayGame, PlayGame);
        }

        private void InitUI()
        {
            titleUIPanel.SetActive(true);
            stageSelectPanel.SetActive(false);
            bubbleContainer.gameObject.SetActive(false);
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
                EventManager.TriggerEvent(ActionEvent.PlayGame);
                EventManager.TriggerEvent(ActionEvent.CreateStage, stageData);
                stageSelectPanel.SetActive(false);
                bubbleContainer.gameObject.SetActive(true);
            });
        }

        private void PlayGame()
        {
            titleUIPanel.SetActive(false);
            stageSelectPanel.SetActive(true);
        }

        private void GoHome()
        {
            stageSelectPanel.SetActive(false);
            titleUIPanel.SetActive(true);
        }
    }
}