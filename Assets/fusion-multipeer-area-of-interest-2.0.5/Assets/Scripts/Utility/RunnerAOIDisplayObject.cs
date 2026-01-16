namespace Fusion
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    
    /// <summary>
    /// Creates a wireframe cube made of smaller ones.
    /// </summary>
    [ExecuteInEditMode()]
    public class RunnerAOIDisplayObject : MonoBehaviour
    {
        private Transform t;

        public Transform[] edges = new Transform[0];

        public Vector3 center;
        public Vector3 size;

        private Vector3 previousCenter;
        private Vector3 previousSize;
        private float previousEdgeScale;
        public float defaultEdgeScale;

        public bool isOverallGrid;

        public Transform centerSquare;

        public AoIMPSettings settings;

        private void Awake()
        {
            t = transform;
        }

        public void SetCenterAndSize(Vector3 center, Vector3 size, bool active, bool showCenterSquare)
        {
            this.center = center;
            this.size = size;

            bool canDisplay;
            if (isOverallGrid)
                canDisplay = active && settings.ShowAreaOfInterestGrid;
            else
                canDisplay = active && settings.ShowActiveServerZones;

            gameObject.SetActive(canDisplay);

            centerSquare.gameObject.SetActive(showCenterSquare && settings.ShowPlayerInterest);
        }

        // Updates the size of the cube but only if one of the previous parameters has changed.
        private void Update()
        {
            if (previousSize == size && previousCenter == center && defaultEdgeScale == previousEdgeScale)
            {
                return;
            }

            previousCenter = center;
            previousSize = size;
            previousEdgeScale = defaultEdgeScale;

            t.position = center;

            centerSquare.localScale = size;

            for (int i = 0; i < edges.Length; i++)
            {
                if (i < 4)
                {
                    edges[i].localScale = new Vector3(size.x, previousEdgeScale, previousEdgeScale);

                    float yM = i % 2 == 0 ? 1 : -1;
                    float zM = i < 2 ? 1 : -1;
                    edges[i].localPosition = new Vector3(0, yM * size.y * 0.5f, zM * size.z * 0.5f);
                }
                else if (i < 8)
                {
                    edges[i].localScale = new Vector3(previousEdgeScale, previousEdgeScale, size.z);

                    float yM = i % 2 == 0 ? 1 : -1;
                    float xM = i < 6 ? 1 : -1;
                    edges[i].localPosition = new Vector3(xM * size.x * 0.5f, yM * 0.5f * size.y, 0);
                }
                else
                {
                    edges[i].localScale = new Vector3(previousEdgeScale, size.y, previousEdgeScale);

                    float zM = i % 2 == 0 ? 1 : -1;
                    float xM = i < 10 ? 1 : -1;
                    edges[i].localPosition = new Vector3(xM * size.x * 0.5f, 0, zM * size.z * 0.5f);
                }
            }
        }
    }
}