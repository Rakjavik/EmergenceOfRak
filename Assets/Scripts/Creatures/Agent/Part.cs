using System;
using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public class Part
    {
        public float UpdateEvery; // How often Part Updates in seconds
        public CreaturePart PartType; // Rough category for type of limb/part
        public Transform PartTransform; // Transform of the single part
        public bool Enabled { get; protected set; } // Enabled
        public AudioClip audioClip { get; protected set; }

        protected Creature parentCreature;
        protected float sinceLastUpdate;
        protected Rigidbody attachedBody;
        protected CreatureAgent attachedAgent;
        protected Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables; // Misc Constants

        public Part(CreaturePart creaturePart,Transform transform, float updateEvery)
        {
            this.PartType = creaturePart;
            this.PartTransform = transform;
            this.UpdateEvery = updateEvery;
            this.parentCreature = transform.GetComponentInParent<Creature>();
            this.attachedAgent = parentCreature.GetCreatureAgent();
            miscVariables = MiscVariables.GetAgentMiscVariables(parentCreature);
            audioClip = CreatureConstants.GetCreaturePartAudioClip(parentCreature.GetBaseSpecies(), creaturePart);
            Enabled = true;
        }

        public virtual void Disable()
        {
            if (Enabled)
            {
                Enabled = false;
            }
        }
        public virtual void Enable()
        {
            if (Enabled) Debug.LogError("Call to enable part when already enabled");
            Enabled = true;
        }

        // Called from Agent Object //
        public void Update()
        {
            if (!Enabled) return;
            sinceLastUpdate += Time.deltaTime;
            if (sinceLastUpdate >= UpdateEvery)
            {
                sinceLastUpdate = 0;
                if(this is EnginePart)
                {
                    EnginePart thisMovementPart = (EnginePart)this;
                    thisMovementPart.UpdateMovePart();
                }
                else if (this is TurnPart)
                {
                    TurnPart thisTurnPart = (TurnPart)this;
                    thisTurnPart.turn();
                }
                else
                {
                    AnimationPart thisAnimationPart = (AnimationPart)this;
                    thisAnimationPart.UpdateAnimationPart(parentCreature.GetCurrentAction());
                }
            }
        }
        private bool arrayContains(ActionStep.Actions action, ActionStep.Actions[] list)
        {
            foreach (ActionStep.Actions singleAction in list)
            {
                if (singleAction == action)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public enum MovementState
    {
        FORWARD, // Max Force on positive end
        IDLE, // 0 for X and Z, Hover for Y
        NONE, // Should never be activated
        POWER_DOWN, // Force degrades slowly then turns engines UNINTIALIZED when 0 is hit
        REVERSE, // Max negative force
        STARTING, // Coming from UNINITIALIZED state
        UNINITIALIZED // Shut down
    }
    public enum Direction { X, Y, Z }
}