using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolControl
{
    public class AutoDisablePoolObject : PoolObject
    {
        [SerializeField] private float disableTime = 1;

        private void OnEnable()
        {
            AwaitDisableTime().Forget();
        }

        private async UniTask AwaitDisableTime()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disableTime), cancellationToken: destroyCancellationToken);
            gameObject.SetActive(false);
        }
    }
}