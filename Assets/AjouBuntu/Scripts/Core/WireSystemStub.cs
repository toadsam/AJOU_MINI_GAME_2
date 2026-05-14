using UnityEngine;

namespace AjouBuntu.Core
{
    public sealed class WireSystemStub : MonoBehaviour
    {
        [SerializeField] private bool enabledByFeatureFlag;

        public void Initialize(bool wireEnabled)
        {
            enabledByFeatureFlag = wireEnabled;
            enabled = wireEnabled;
        }
    }
}
