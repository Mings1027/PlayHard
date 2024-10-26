using DataControl;
using UnityEngine;

namespace UIControl
{
    public class StageSelectManager : MonoBehaviour
    {
        private StageButton[] _stageButtons;
        [SerializeField] private GameObject stageButtonPrefab;
        [SerializeField] private StageData[] stageDataList;

        private void Awake()
        {
            _stageButtons = new StageButton[stageDataList.Length];
            for (int i = 0; i < stageDataList.Length; i++)
            {
                _stageButtons[i] = Instantiate(stageButtonPrefab, transform).GetComponent<StageButton>();
                _stageButtons[i].SetData(stageDataList[i]);
            }
        }

        public void SelectStage(int index)
        {
            _stageButtons[index].SelectStage();
        }
    }
}