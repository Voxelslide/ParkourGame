using StarterAssets;
using System.Buffers.Text;
using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

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

    controller.SetFallTimeoutDelta(controller.FallTimeout);
    var animator = controller.GetAnimator();
    if (animator != null)
    {
      animator.SetBool("Grounded", true);
      animator.SetBool("FreeFall", false);
    }

    groundedMovementStateMachine = new StateMachine();
    // choose starting substate based on input
    if (controller.GetInput().move != Vector2.zero)
    {
      if (controller.GetInput().sprint)
        groundedMovementStateMachine.Initialize(new SprintState(controller, groundedMovementStateMachine));
      else
        groundedMovementStateMachine.Initialize(new WalkState(controller, groundedMovementStateMachine));
    }
    else
    {
      groundedMovementStateMachine.Initialize(new IdleState(controller, groundedMovementStateMachine));
    }
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

    // Delegate input handling to sub-state machine
    groundedMovementStateMachine.HandleInput();
  }



  //LogicUpdate
  public override void LogicUpdate()
  {
    // Check for falling (not grounded anymore) -> exit Grounded
    if (!controller.IsGrounded())
    {
      var fallDelta = controller.GetFallTimeoutDelta();
      if (fallDelta > 0.0f)
      {
        controller.SetFallTimeoutDelta(fallDelta -= Time.deltaTime);
      }
      else
      {
        // Fall timeout expired — officially falling
        stateMachine.ChangeState(new FallState(controller, stateMachine));
        return;
      }
      stateMachine.ChangeState(new FallState(controller, stateMachine));
      return;
    }

    // Reset fall timer if grounded
    controller.SetFallTimeoutDelta(controller.FallTimeout);
    groundedMovementStateMachine.LogicUpdate();
  }

  //PhysicsUpdate
  public override void PhysicsUpdate()
  {
    controller.SetVerticalVelocity(-4f);
    groundedMovementStateMachine.PhysicsUpdate();
  }


  //Exit
  public override void Exit()
  {

    // Store the player's horizontal momentum before leaving the ground
    Vector3 horizontalVelocity = new Vector3(
        controller.GetCharacterController().velocity.x,
        0f,
        controller.GetCharacterController().velocity.z
    );

    controller.SetMomentum(horizontalVelocity);


    groundedMovementStateMachine.CurrentState.Exit();
    Debug.Log("Exited GroundedMovement");
  }

}
