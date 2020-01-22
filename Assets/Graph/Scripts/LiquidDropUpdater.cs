using UnityEngine;

namespace RDSystem
{
    [ExecuteAlways]
    public class LiquidDropUpdater : MonoBehaviour
    {
        [SerializeField] CustomRenderTexture _texture;
        int _stepsPerFrame = 1;
        bool m_update = false;
        bool m_hit = false;
        [HideInInspector]
        public Vector2 hitPosition;
        [HideInInspector]
        public Color hitColor = Color.green;
        public float simulationDuration = 4;

        void Start()
        {   
            _texture.material.SetColor("_Color1", hitColor);
            _texture.material.SetVector("_Position", new Vector4(hitPosition.x, hitPosition.y, 0, 0));
            _texture.Initialize();
        }

        void Update()
        {
            if (m_hit == true)
            {
                m_update = true;
                InitTexture(_texture);
                Invoke("Stop", simulationDuration);
                m_hit = false;
            }
            else if (m_update == true)
            {
                UpdateTexture(_texture);
            }
        }

        void InitTexture(CustomRenderTexture texture)
        {
            texture.material.SetVector("_Position", new Vector4(hitPosition.x, hitPosition.y, 0, 0));
            texture.material.SetColor("_Color1", hitColor);
            texture.shaderPass = 1;
            texture.Update();
        }

        void UpdateTexture(CustomRenderTexture texture)
        {
            texture.shaderPass = 0;
            texture.Update(_stepsPerFrame);
        }

        void Stop()
        {
            m_update = false;
        }

        public void Hit()
        {
            Color liquidColor = _texture.material.GetColor("_Color1");
            if (hitColor != liquidColor)
                m_hit = true;
        }
    }
}