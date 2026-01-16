using UnityEngine;

namespace Fusion.Samples.Stage
{
    public class Emotes : MonoBehaviour
    {
        [SerializeField] private float emoteLifeDuration = 10f;
        [SerializeField] private float emoteScaleDuration = 3f;
        [SerializeField] private float emoteHorizontalSpeed = 2f;
        [SerializeField] private float emoteVerticalSpeed = 2f;
        [SerializeField] private float emoteScaleSpeed = 2f;
        [SerializeField] private float emoteHorizontalMovement = 0.005f;

        private Vector3 newposition;
        private float scale;
        private float startTime;
        private float alea;

        // Start is called before the first frame update
        void Start()
        {
            alea = Random.Range(0.5f, 1.5f);
            startTime = Time.time;
            scale = 0.1f;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > startTime + emoteLifeDuration)
            {
                Destroy(gameObject);
            }
            else
            {
                newposition = transform.position;
                newposition.y += Time.deltaTime * emoteVerticalSpeed;
                newposition.x += emoteHorizontalMovement * Mathf.Sin(Time.time * emoteHorizontalSpeed * alea);
                transform.position = newposition;

                if (Time.time < startTime + emoteScaleDuration)
                {
                    scale += Time.deltaTime * emoteScaleSpeed;
                    gameObject.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
    }
}
