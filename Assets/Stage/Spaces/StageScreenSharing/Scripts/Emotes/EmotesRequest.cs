using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    public class EmotesRequest : MonoBehaviour
    {
        RigInfo rigInfo;
        EmotesSpawner emoteSpawner;

        private void Awake()
        {
            if (rigInfo == null) rigInfo = RigInfo.FindRigInfo(allowSceneSearch: true);
        }

        void PrepareNetworkRig()
        {
            if (emoteSpawner == null && rigInfo != null && rigInfo.localNetworkedRig != null) emoteSpawner = rigInfo.localNetworkedRig.GetComponentInChildren<EmotesSpawner>();
        }


        public void OnClickHappy()
        {
            SpawnEmoticon(1);
        }

        public void OnClickUnhappy()
        {
            SpawnEmoticon(2);
        }
        public void OnClickClap()
        {
            SpawnEmoticon(3);
        }

        private void SpawnEmoticon(int emoticonPrefabID)
        {
            PrepareNetworkRig();
            if (emoteSpawner == null) return;
            emoteSpawner.SpawnEmoticon(emoticonPrefabID, transform.position, Quaternion.identity);
        }
    }
}