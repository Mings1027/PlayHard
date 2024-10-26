using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button[] animatedButtons;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject titleUIPanel;
        [SerializeField] private StageSelectManager stageSelectManager;

        private void Awake()
        {
            InitUI();
            InitButtonAnimations();
            InitButtonEvents();
        }

        private void InitUI()
        {
            titleUIPanel.SetActive(true);
            stageSelectManager.gameObject.SetActive(false);
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
        }

        private void PlayGame()
        {
            titleUIPanel.SetActive(false);
            stageSelectManager.gameObject.SetActive(true);
            stageSelectManager.gameObject.SetActive(false);
            stageSelectManager.SelectStage(0);
        }

        private void GoHome()
        {
            stageSelectManager.gameObject.SetActive(false);
            titleUIPanel.SetActive(true);
        }
    }
}