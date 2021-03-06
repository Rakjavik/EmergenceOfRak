﻿using System.Collections.Generic;

namespace rak.creatures
{
    public abstract class MiscVariables
    {
        public enum AgentMiscVariables
        {
            Part_Flight_Max_Vel_Magnitude_Brake_Multiplier,
            Part_Flight_Angular_Velocity_Brake_When_Over,
            Part_Flight_Max_Vel_Mag_Before_Brake,
            Part_Flight_Reverse_Engine_If_Colliding_In,
            Part_Flight_X_Engine_Kick_In_When_Faster_Than,
            Part_Flight_Y_Engine_Dist_To_Act_UP_Mult,
            Part_Flight_Z_Engine_Dist_To_Act_FWD_Factor,
            Part_Flight_Y_Engine_PowerDown_Until_Vel_At_Most,
            Part_Flight_Y_Engine_PowerDown_Reduction_Factor,
            Part_Flight_Maintain_Below_Velocity,
            Part_Flight_Brake_When_Going_Wrong_Direction_If_Vel,
            MoveVar_Start_Up_Time_In_Minutes,
            Agent_Is_Stuck_If_Moved_Less_Than_In_One_Sec,
            Agent_Detect_Collision_Vel_Distance,
            Agent_Detect_Collision_Direction_Distance,
            Agent_Brake_If_Colliding_In,
            Part_Flight_Halt_Forward_Movement_If_Object_Is_Distance
        }

        public static Dictionary<AgentMiscVariables, float> GetAgentMiscVariables(Creature creature)
        {
            return GetAgentMiscVariables(creature.getSpecies().getBaseSpecies());
        }
        public static Dictionary<AgentMiscVariables, float> GetAgentMiscVariables(BASE_SPECIES species)
        {
            Dictionary<AgentMiscVariables, float> miscVariables = new Dictionary<AgentMiscVariables, float>();
            if (species == BASE_SPECIES.Gnat)
            {
                miscVariables.Add(AgentMiscVariables.Agent_Detect_Collision_Vel_Distance, 150);
                miscVariables.Add(AgentMiscVariables.Agent_Detect_Collision_Direction_Distance, 15);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Halt_Forward_Movement_If_Object_Is_Distance, 3);
                miscVariables.Add(AgentMiscVariables.Agent_Brake_If_Colliding_In, 2);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Max_Vel_Magnitude_Brake_Multiplier, 1);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Angular_Velocity_Brake_When_Over, 2);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Max_Vel_Mag_Before_Brake, 20);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Reverse_Engine_If_Colliding_In, 5);
                miscVariables.Add(AgentMiscVariables.Part_Flight_X_Engine_Kick_In_When_Faster_Than, .3f);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Y_Engine_Dist_To_Act_UP_Mult, 2);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Z_Engine_Dist_To_Act_FWD_Factor, 2);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Y_Engine_PowerDown_Until_Vel_At_Most, -.3f);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Y_Engine_PowerDown_Reduction_Factor, 10);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Maintain_Below_Velocity, .15f);
                miscVariables.Add(AgentMiscVariables.Part_Flight_Brake_When_Going_Wrong_Direction_If_Vel, 20);
                miscVariables.Add(AgentMiscVariables.MoveVar_Start_Up_Time_In_Minutes, .1f);
                miscVariables.Add(AgentMiscVariables.Agent_Is_Stuck_If_Moved_Less_Than_In_One_Sec,10);
            }
            return miscVariables;
        }

        public enum CreatureMiscVariables
        {
            Observe_Distance,
            Agent_Locate_Sleep_Area_BoxCast_Size_Multipler,
            Agent_MoveTo_Raycast_For_Target_When_Distance_Below,
        }
        public static Dictionary<CreatureMiscVariables,float> GetCreatureMiscVariables(Creature creature)
        {
            Dictionary<CreatureMiscVariables, float> miscVariables = new Dictionary<CreatureMiscVariables, float>();
            miscVariables.Add(CreatureMiscVariables.Observe_Distance, 100);
            miscVariables.Add(CreatureMiscVariables.Agent_Locate_Sleep_Area_BoxCast_Size_Multipler, 50);
            miscVariables.Add(CreatureMiscVariables.Agent_MoveTo_Raycast_For_Target_When_Distance_Below, 1);
            return miscVariables;
        }
    }
}