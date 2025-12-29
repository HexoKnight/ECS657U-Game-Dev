using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Root composite state for when the player is alive and active.
    /// Manages high-level modes: Free Control (Normal) vs Path Following (Cinematic/Spline).
    /// </summary>
    public class AliveState : PlayerCompositeState
    {
        private readonly FreeControlState freeControlState;
        private readonly PathFollowState pathFollowState;
        
        public override string StateName => "Alive";
        
        public FreeControlState FreeControl => freeControlState;
        public PathFollowState PathFollow => pathFollowState;
        
        public AliveState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
            freeControlState = new FreeControlState(stateMachine, context);
            pathFollowState = new PathFollowState(stateMachine, context);
        }
        
        protected override IState GetInitialSubstate() => freeControlState;
        
        public void TransitionToFreeControl()
        {
            ChangeSubstate(freeControlState);
        }
        
        public void TransitionToPathFollow()
        {
            ChangeSubstate(pathFollowState);
        }
        
        public override void CheckTransitions()
        {
            // Transitions are mostly driven by external events (StartStaticSpline)
            // or completion callbacks (Path Finished).
        }
    }
}
