using UnityEngine;

namespace rak.creatures
{
    public class PartEngineVariables
    {
        public Vector3 MoveDirection { get; protected set; }
        public PartMovesWith PartMovesWith { get; set; }
        public RigidbodyConstraints currentConstraints { get; protected set; }

        public PartEngineVariables(Vector3 moveDirection, PartMovesWith partMovesWith)
        {
            MoveDirection = moveDirection;
            PartMovesWith = partMovesWith;
        }
    }
}