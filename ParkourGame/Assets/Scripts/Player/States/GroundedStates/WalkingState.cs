using UnityEngine;

public class WalkingState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


/*
 public class WalkState : PlayerState
{
    public WalkState(ThirdPersonController controller, StateMachine stateMachine)
        : base(controller, stateMachine) { }

    public override void Enter()
    {
        controller.Animator.Play("Walk");
    }

    public override void LogicUpdate()
    {
        controller.MoveCharacter(controller.MoveInput, controller.walkSpeed);

        if (controller.MoveInput.magnitude < 0.1f)
            stateMachine.ChangeState(new IdleState(controller, stateMachine));
    }
}

 
 */