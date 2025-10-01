using StarterAssets;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class GroundedState : PlayerState
{


  private StateMachine groundedMovementStateMachine;

  //Constructor
  public GroundedState(ThirdPersonController controller, StateMachine stateMachine)
  : base(controller, stateMachine) { }

  //Enter
  public override void Enter()
  {
    Debug.Log("Entered GroundedMovement");
    groundedMovementStateMachine = new StateMachine();
    groundedMovementStateMachine.Initialize(new IdleState(controller, groundedMovementStateMachine));
  }




  //HandleInput
  public override void HandleInput()
  {
    var input = controller.GetInput();

    // Check jump -> exit Grounded entirely
    if (input.jump)
    {
      //Coming soon
      //stateMachine.ChangeState(new JumpState(controller, stateMachine));
      return;
    }

    // Check falling (not grounded anymore) -> exit Grounded
    if (!controller.IsGrounded())
    {
      //Coming soon
      //stateMachine.ChangeState(new FallState(controller, stateMachine));
      return;
    }

    // Delegate input handling to sub-state machine
    groundedMovementStateMachine.HandleInput();
  }



  //LogicUpdate
  public override void LogicUpdate()
  {
    groundedMovementStateMachine.LogicUpdate();
  }

  //PhysicsUpdate
  public override void PhysicsUpdate()
  {
    groundedMovementStateMachine.PhysicsUpdate();
  }

  //Exit
  public override void Exit()
  {
    Debug.Log("Exited GroundedMovement");
  }

}
