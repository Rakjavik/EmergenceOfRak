using UnityEngine;

namespace rak.creatures
{
    public class RAKUpdateMeshRendererWithKinematic : MonoBehaviour
    {
        public Rigidbody rb;
        public MeshRenderer mesh;

        private void Update()
        {
            if (rb.isKinematic && !mesh.enabled)
                mesh.enabled = true;
            else if (!rb.isKinematic && mesh.enabled)
                mesh.enabled = false;
        }
    }

}
