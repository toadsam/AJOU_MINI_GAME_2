using UnityEngine;

namespace AjouBuntu.Core
{
    public sealed class ObstacleSystemStub : MonoBehaviour
    {
        [SerializeField] private bool enabledByFeatureFlag;

        public void Initialize(bool obstacleEnabled)
        {
            enabledByFeatureFlag = obstacleEnabled;
            enabled = obstacleEnabled;
        }
    }
}
