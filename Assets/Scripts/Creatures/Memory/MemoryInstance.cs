using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures.memory
{
    [Serializable]
    public struct MemoryInstance
    {
        public Verb Verb;
        public byte InvertVerb;
        public Entity Subject;
        public float TimeStamp;
        public int Iterations;
        public byte Edible { get; private set; }
        public float3 Position;
        public Thing.Base_Types SubjectType;
        public float SubjectMass;

        public MemoryInstance(Verb verb, Entity subject, bool invertVerb, float timestamp,
            Thing.Base_Types subjectType, ConsumptionType creatureConsumeType,float3 position,float subjectMass)
        {
            if (invertVerb)
                this.InvertVerb = 1;
            else
                this.InvertVerb = 0;
            this.Verb = verb;
            this.Subject = subject;
            this.TimeStamp = timestamp;
            Iterations = 0;
            Edible = 0;
            Position = position;
            SubjectType = subjectType;
            SubjectMass = subjectMass;
        }
        public void RefreshEdible(ConsumptionType creatureConsumeType)
        {
            if (creatureConsumeType == ConsumptionType.HERBIVORE)
            {
                if (SubjectType == Thing.Base_Types.PLANT)
                {
                    Edible = 1;
                    return;
                }
            }
            Edible = 0;
        }

        private void setInvertVerb(bool invertVerb)
        {
            if (invertVerb)
                this.InvertVerb = 1;
            else
                this.InvertVerb = 0;
        }
        public bool GetInvertVerb()
        {
            return getInvertVerb(InvertVerb);
        }
        private bool getInvertVerb(short invertVerb)
        {
            if (invertVerb == 0)
                return false;
            else
                return true;
        }
        public void MakeEmpty()
        {
            Verb = Verb.NA;
        }
        public bool IsEmpty()
        {
            if (Verb == Verb.NA)
                return true;
            return false;
        }
        public static MemoryInstance GetNewEmptyMemory()
        {
            return new MemoryInstance(Verb.NA, Entity.Null, false,0,Thing.Base_Types.NA,
                ConsumptionType.CARNIVORE,float3.zero,0);
        }
        public void ReplaceMemory(Verb verb, Entity subject, bool invertVerb)
        {
            this.Verb = verb;
            this.Subject = subject;
            setInvertVerb(invertVerb);
            TimeStamp = DateTime.Now.ToBinary();
            Iterations = 0;
        }

        public void AddIteration() { Iterations++; }
        public bool IsSameAs(Verb verb, Entity subject, bool invertVerb)
        {
            if (subject != Entity.Null)
            {
                if (invertVerb == getInvertVerb(this.InvertVerb) &&
                    subject == this.Subject &&
                    verb == this.Verb)
                {
                    return true;
                }
            }
            return false;
        }
        public void SetNewMemory(MemoryInstance newMemory)
        {
            Verb = newMemory.Verb;
            InvertVerb = newMemory.InvertVerb;
            Subject = newMemory.Subject;
            TimeStamp = newMemory.TimeStamp;
            Iterations = newMemory.Iterations;
            Edible = newMemory.Edible;
        }
    }
}