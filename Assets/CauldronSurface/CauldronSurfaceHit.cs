using UnityEngine;

namespace RDSystem
{
    public class CauldronSurfaceHit : MonoBehaviour
    {
        public RDSystem.LiquidDropUpdater updater;
        public int materialID = 0;
        public string colorPropertyName = "_BaseColor";
        public Collider m_collider;

        private void OnCollisionEnter(Collision collision)
        {
            var fillRate = collision.gameObject.GetComponent<MeshRenderer>().materials[materialID].GetFloat("FillingRate");
            if(fillRate>0)
            {
                var pos = gameObject.transform.InverseTransformPoint(collision.contacts[0].point);
                updater.hitPosition = new Vector2(pos.x + 0.5f, pos.y + 0.5f);
                updater.hitColor = collision.gameObject.GetComponent<MeshRenderer>().materials[materialID].GetColor(colorPropertyName);
                updater.Hit();
            }
        }
    }
}