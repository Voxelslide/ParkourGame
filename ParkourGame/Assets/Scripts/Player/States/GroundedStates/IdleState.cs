using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


 public class IdleState : PlayerState
{
  public IdleState(ThirdPersonController controller, StateMachine stateMachine)
   : base(controller, stateMachine) { }

  public override void Enter()
  {
    Debug.Log("Entered Idle State");
  }




  public override void HandleInput()
  {
    var input = controller.GetInput();

    // If player starts moving, leave Idle state
    if (input.move != Vector2.zero && !input.sprint)
    {
      stateMachine.ChangeState(new WalkState(controller, stateMachine));
      return;
    }

    if (input.move != Vector2.zero && input.sprint)
    {
      stateMachine.ChangeState(new SprintState(controller, stateMachine));
      return;
    }

    if (input.jump)
    {
      //Coming soon
      //stateMachine.ChangeState(new JumpState(controller, stateMachine));
      return;
    }
  }


  public override void LogicUpdate()
  {
    // Could put "check if grounded" or "fall detection" here if not using a parent GroundedState
    //but i probably will?

    //Smooth out stopping animation blend
    var animator = controller.GetAnimator();
    if (animator != null)
    {
      float currentBlend = animator.GetFloat("Speed");
      float smoothedBlend = Mathf.Lerp(currentBlend, 0f, Time.deltaTime * controller.SpeedChangeRate);
      animator.SetFloat("Speed", smoothedBlend);
      animator.SetFloat("MotionSpeed", 0f); // MotionSpeed can stay at 0 since there's no input
    }
  }


  public override void PhysicsUpdate()
  {
    // Stop horizontal movement while idle
    var characterController = controller.GetCharacterController();
    characterController.Move(new Vector3(0f, controller.GetVerticalVelocity(), 0f) * Time.deltaTime);
  }

  public override void Exit()
  {
    //Exit idle anim ("If it's not done automatically")
    Debug.Log("Exited Idle State");

  }




}


