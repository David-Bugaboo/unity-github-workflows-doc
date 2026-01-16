using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Animation when pointing over a seat
     */
    public class SeatAutoRotate : MonoBehaviour
    {
        public float speedRotation = 150f;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime * speedRotation);
        }
    }
}
