using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button[] animatedButtons;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject titleUIPanel;
        [SerializeField] private StageSelectManager stageSelectManager;
        [SerializeField] private GameObject inGameUIPanel;
        [SerializeField] private RectTransform remainingBubbleUI;
        [SerializeField] private TMP_Text remainingCountText;

        [SerializeField] private GameObject touchBlockPanel;
        [SerializeField] private CanvasGroup gameOverPanel;
        [SerializeField] private Button goHomeButton;

        private void Awake()
        {
            InitUI();
            InitButtonAnimations();
            InitButtonEvents();
        }

        private void OnEnable()
        {
            EventManager.AddEvent<int>(ActionEvent.SetRemainingCountText, SetRemainingCountText);
            EventManager.AddEvent<Vector3>(ActionEvent.DisplayInGamePanel, DisplayInGamePanel);
            EventManager.AddEvent(ActionEvent.DisplayTouchBlockPanel, DisplayTouchBlockPanel);
            EventManager.AddEvent(ActionEvent.DisplayGameOverPanel, DisplayGameOverPanel);
        }

        private void OnDisable()
        {
            EventManager.RemoveEvent<int>(ActionEvent.SetRemainingCountText, SetRemainingCountText);
            EventManager.RemoveEvent<Vector3>(ActionEvent.DisplayInGamePanel, DisplayInGamePanel);
            EventManager.RemoveEvent(ActionEvent.DisplayTouchBlockPanel, DisplayTouchBlockPanel);
            EventManager.RemoveEvent(ActionEvent.DisplayGameOverPanel, DisplayGameOverPanel);
        }

        private void InitUI()
        {
            titleUIPanel.SetActive(true);
            stageSelectManager.gameObject.SetActive(false);
            inGameUIPanel.SetActive(false);
            touchBlockPanel.SetActive(false);
            gameOverPanel.gameObject.SetActive(false);
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
            playButton.onClick.AddListener(PlayGame);
            goHomeButton.onClick.AddListener(GoHome);
        }

        private void PlayGame()
        {
            titleUIPanel.SetActive(false);
            stageSelectManager.gameObject.SetActive(true);
            SelectStage();
        }

        private void SelectStage()
        {
            stageSelectManager.gameObject.SetActive(false);
            stageSelectManager.SelectStage(0);
        }

        private void DisplayInGamePanel(Vector3 shooterPosition)
        {
            inGameUIPanel.SetActive(true);
            remainingBubbleUI.position = Camera.main.WorldToScreenPoint(shooterPosition);
        }

        private void SetRemainingCountText(int count) => remainingCountText.text = count.ToString();

        private void DisplayTouchBlockPanel()
        {
            inGameUIPanel.SetActive(false);
            touchBlockPanel.SetActive(true);
        }

        private void DisplayGameOverPanel()
        {
            gameOverPanel.gameObject.SetActive(true);
            var gameOverRect = gameOverPanel.transform.GetComponent<RectTransform>();
            gameOverPanel.DOFade(1, 0.25f).From(0);
            gameOverRect.DOAnchorPosY(0, 0.5f).From(new Vector2(0, -10));
        }

        private void GoHome()
        {
            stageSelectManager.gameObject.SetActive(false);
            touchBlockPanel.SetActive(false);
            gameOverPanel.gameObject.SetActive(false);
            titleUIPanel.SetActive(true);
        }
    }
}