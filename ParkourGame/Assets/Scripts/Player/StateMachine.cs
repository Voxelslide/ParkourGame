using UnityEngine;

public class StateMachine
{
	public PlayerState CurrentState { get; private set; }

	public void Initialize(PlayerState startingState)
	{
		CurrentState = startingState;
		CurrentState.Enter();
	}

	public void ChangeState(PlayerState newState)
	{
		CurrentState.Exit();
		CurrentState = newState;
		newState.Enter();
	}

	public void HandleInput() => CurrentState.HandleInput();
	public void LogicUpdate() => CurrentState.LogicUpdate();
	public void PhysicsUpdate() => CurrentState.PhysicsUpdate();
}
