using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button[] animatedButtons;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject titleUIPanel;

        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private StageData stageData;

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
                UniTaskEventManager.TriggerAsync(UniTaskEvent.CreateStage, stageData).Forget();
                stageSelectPanel.SetActive(false);
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