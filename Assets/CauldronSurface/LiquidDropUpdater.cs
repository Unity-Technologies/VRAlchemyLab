using UnityEngine;

namespace RDSystem
{
    [ExecuteAlways]
    public class LiquidDropUpdater : MonoBehaviour
    {
        [SerializeField] CustomRenderTexture _texture;
        [SerializeField] CustomRenderTexture _textureCube;
        [SerializeField, Range(1, 16)] int _stepsPerFrame = 4;
        bool m_update = false;
        bool m_hit = false;
        public Vector2 hitPosition;
        public Color hitColor = Color.green;
        public float simulationDuration = 2;

        void Start()
        {   
            _texture.material.SetColor("_Color1", hitColor);
            _texture.material.SetVector("_Position", new Vector4(hitPosition.x, hitPosition.y, 0, 0));
            _texture.Initialize();
            _textureCube.Initialize();
        }

        void Update()
        {
            if (m_hit == true)
            {
                m_update = true;
                InitTexture(_texture);
                InitTexture(_textureCube);
                Invoke("Stop", simulationDuration);
                m_hit = false;
            }
            else if (m_update == true)
            {
                UpdateTexture(_texture);
                UpdateTexture(_textureCube);
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