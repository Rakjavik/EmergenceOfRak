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
        private EntityManager em;

        public EnginePart(PartAudioPropToModify audioProp, CreaturePart creaturePart,
            Transform transform,CreatureLocomotionType locoType, float updateEvery) : 
            base(creaturePart, transform, updateEvery)
        {
            this.audioProp = audioProp;
            GameObject parentGO = PartTransform.GetComponentInParent<Thing>().gameObject;
            partAudio = parentGO.AddComponent<AudioSource>();
            cf = parentGO.AddComponent<ConstantForce>();
            partAudio.enabled = false;
            em = Unity.Entities.World.Active.EntityManager;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            if (audioProp == PartAudioPropToModify.PITCH)
            {
                EngineSound es = em.GetComponentData<EngineSound>(parentCreature.ThingEntity);
                partAudio.pitch = es.CurrentLevel;
                EngineConstantForce engineForce = em.GetComponentData<EngineConstantForce>(parentCreature.ThingEntity);
                cf.relativeForce = engineForce.CurrentForce;
            }
        }
    }
}