namespace Fusion
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This Component works similar to RunnerAoIGizmos but creates and manages a pool of cubes used to display the information.
    /// </summary>
    [RequireComponent(typeof(NetworkRunner))]
    [ScriptHelp(BackColor = ScriptHeaderBackColor.Sand)]
    [DisallowMultipleComponent]
    public class RunnerAOIDisplay : SimulationBehaviour
    {
        public RunnerAOIDisplayObject displayObjectPrefab;

        public List<RunnerAOIDisplayObject> displayObjectPool;

        public RunnerAOIDisplayObject mainGridDisplay;

        public AoIMPSettings settings;

        private List<(Vector3 center, Vector3 size, int playerCount, int objectCount)> _reusableGizmoData;

        private void LateUpdate()
        {
            NetworkRunner runner = Runner;
            if ((object)runner == null || !runner.IsRunning)
                return;

            if (!runner.IsServer)
            {
                mainGridDisplay.SetCenterAndSize(Vector3.zero, settings.AreaOfInterestGrid, false, false);
                enabled = false;
                return;
            }

            mainGridDisplay.SetCenterAndSize(Vector3.zero, settings.AreaOfInterestGrid, true, false);

            var datas = _reusableGizmoData ??= new List<(Vector3 center, Vector3 size, int playerCount, int objectCount)>();

            runner.GetAreaOfInterestGizmoData(datas);

            Debug.Log("DC:  " + datas.Count);

            int i;
            for (i = 0; i < datas.Count; i++)
            {
                if (i >= displayObjectPool.Count)
                {
                    displayObjectPool.Add(Instantiate(displayObjectPrefab, transform));
                }

                var data = datas[i];

                if (datas[i].objectCount > 0 || datas[i].playerCount > 0)
                {
                    displayObjectPool[i].SetCenterAndSize(data.center, data.size, true, datas[i].playerCount > 0);
                }
                else
                {
                    displayObjectPool[i].SetCenterAndSize(data.center, data.size, false, false);
                }
            }

            while (i < displayObjectPool.Count)
            {
                displayObjectPool[i++].SetCenterAndSize(Vector3.zero, Vector3.zero, false, false);
            }
        }
    }
}