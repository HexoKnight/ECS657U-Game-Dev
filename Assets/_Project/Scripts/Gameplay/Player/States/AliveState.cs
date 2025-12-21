using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Root composite state for when the player is alive and active.
    /// Contains Locomotion as initial substate.
    /// </summary>
    public class AliveState : PlayerCompositeState
    {
        private readonly LocomotionState locomotionState;
        
        public override string StateName => "Alive";
        
        public AliveState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
            locomotionState = new LocomotionState(stateMachine, context);
        }
        
        protected override IState GetInitialSubstate() => locomotionState;
        
        public override void CheckTransitions()
        {
            // TODO: Transition to Dead state when health <= 0
            // For now, stay alive
        }
    }
}
