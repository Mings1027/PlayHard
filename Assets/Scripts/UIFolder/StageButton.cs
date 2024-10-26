using Cysharp.Threading.Tasks;
using DataControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class StageButton : MonoBehaviour
    {
        private StageData _stageData;

        public void SetData(StageData stageData)
        {
            _stageData = stageData;
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
                UniTaskEventManager.TriggerAsync(UniTaskEvent.CreateStage, _stageData).Forget());

            var stageText = GetComponentInChildren<TMP_Text>();
            stageText.text = stageData.name;
        }

        public void SelectStage()
        {
            UniTaskEventManager.TriggerAsync(UniTaskEvent.CreateStage, _stageData).Forget();
        }
    }
}