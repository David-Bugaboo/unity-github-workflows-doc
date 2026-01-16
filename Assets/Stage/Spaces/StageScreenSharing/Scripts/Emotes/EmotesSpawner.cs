using UnityEngine;

namespace Fusion.Samples.Stage
{
    public class EmotesSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject happyPrefab;
        [SerializeField] private GameObject unhappyPrefab;
        [SerializeField] private GameObject clapPrefab;



        public void SpawnEmoticon(int emoticonPrefabID, Vector3 position, Quaternion rotation)
        {
            LocalSpawnEmoticon(emoticonPrefabID, position, rotation);
            RPC_SpawnEmoticon(emoticonPrefabID, position, rotation);
        }

        [Rpc(InvokeLocal = false)]
        private void RPC_SpawnEmoticon(int emoticonPrefabID, Vector3 emoticonPosition, Quaternion emoticonRotation)
        {
            LocalSpawnEmoticon(emoticonPrefabID, emoticonPosition, emoticonRotation);
        }


        private void LocalSpawnEmoticon(int emoticonPrefabID, Vector3 emoticonPosition, Quaternion emoticonRotation)
        {
            if (emoticonPrefabID == 1)
                Instantiate(happyPrefab, emoticonPosition, emoticonRotation);
            else if (emoticonPrefabID == 2)
                Instantiate(unhappyPrefab, emoticonPosition, emoticonRotation);
            else if (emoticonPrefabID == 3)
                Instantiate(clapPrefab, emoticonPosition, emoticonRotation);
        }
    }
}
