using rak.ecs.ThingComponents;
using rak.world;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures
{
    public enum PartAudioPropToModify { PITCH }
    public class EnginePart : Part
    {
        private PartAudioPropToModify audioProp;
        private AudioSource partAudio;
        private ConstantForce cf;

        public EnginePart(PartAudioPropToModify audioProp, CreaturePart creaturePart,
            Transform transform,CreatureLocomotionType locoType, float updateEvery) : 
            base(creaturePart, transform, updateEvery)
        {
            this.audioProp = audioProp;
            GameObject parentGO = PartTransform.GetComponentInParent<Thing>().gameObject;
            partAudio = parentGO.AddComponent<AudioSource>();
            cf = parentGO.AddComponent<ConstantForce>();
            partAudio.enabled = false;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            if (audioProp == PartAudioPropToModify.PITCH)
            {
                GameObjectEntity goEntity = parentCreature.GetCreatureGOEntity();
                EngineSound es = goEntity.EntityManager.GetComponentData<EngineSound>(goEntity.Entity);
                partAudio.pitch = es.CurrentLevel;
                EngineConstantForce engineForce = goEntity.EntityManager.GetComponentData<EngineConstantForce>(goEntity.Entity);
                cf.relativeForce = engineForce.CurrentForce;
            }
        }
    }
}