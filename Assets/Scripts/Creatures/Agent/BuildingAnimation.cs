using System.Collections.Generic;
using UnityEngine;
using rak.world;

namespace rak.creatures
{
    public class BuildingAnimation
    {
        public BuildingAnimationPiece[] pieces {get;private set;}

        private Transform parentTransform;
        private Building.Building_Type buildingType;

        public BuildingAnimation(Building.Building_Type type, Transform parentTransform)
        {
            this.parentTransform = parentTransform;
            this.buildingType = type;
            pieces = BuildingAnimationPiece.GetPiecesForAnimation(type, parentTransform);
        }
    }

    public class BuildingAnimationPiece
    {
        public static BuildingAnimationPiece[] GetPiecesForAnimation(Building.Building_Type type,Transform parentTransform)
        {
            List<BuildingAnimationPiece> pieces = new List<BuildingAnimationPiece>();
            if(type == Building.Building_Type.House)
            {
                for(int x = 0; x < parentTransform.childCount; x++)
                {
                    for(int y = 0; y < parentTransform.GetChild(x).childCount; y++)
                    {
                        Transform pieceTransform = parentTransform.GetChild(x).GetChild(y);
                        BuildingAnimationPiece piece = new BuildingAnimationPiece(pieceTransform,
                            pieceTransform.position, pieceTransform.rotation, new Vector3(0, 0, 0));
                        pieces.Add(piece);
                    }
                }
            }
            return pieces.ToArray();
        }

        private Transform pieceTransform;
        private Vector3 destinationPoint;
        private Quaternion destinationRotation;

        public BuildingAnimationPiece(Transform transform,Vector3 destination,Quaternion destRotation,
            Vector3 startPosition)
        {
            this.pieceTransform = transform;
            this.destinationPoint = destination;
            this.destinationRotation = destRotation;
            transform.position = startPosition;
        }
    }
}