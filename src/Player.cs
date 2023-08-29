using Godot;

namespace SmoothCamera;
public partial class Player : CharacterBody3D
{
	[Export] private float JumpVelocity = 20f;
	[Export] private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	[Export] private float Acceleration = 5;
	[Export] private float DodgeMultiplier = 20f;
	[Export] private float MoveSpeed = 10f;
	private Vector3 Direction;
	private CameraController CameraController;

	public override void _Ready()
	{
		this.CameraController = GetNode<CameraController>("Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		MovePlayer(delta);
	}

	private void MovePlayer(double delta)
	{
		Vector3 velocity = Velocity.Lerp(Direction * MoveSpeed, Acceleration * ((float)delta));;
		
		Direction = Vector3.Zero;
		Direction.Z = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
		Direction.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		
		Direction = Direction.Rotated(Vector3.Up, this.CameraController.FacingRotation).Normalized();

		velocity.X = Direction.X * MoveSpeed;
		velocity.Z = Direction.Z * MoveSpeed;

		var lookDirection = new Vector2(-velocity.Z, -velocity.X);
		
		if (lookDirection.Length() > 1)
		{
			this.Rotation = new Vector3
			(
				this.Rotation.X, 
				Mathf.LerpAngle
				(
					this.Rotation.Y, lookDirection.Angle(), .1f
				), 
				this.Rotation.Z 
			);
		}

		velocity.Y -= Gravity * (float)delta;
		if (IsOnFloor() && Input.IsActionJustPressed("ui_accept"))
		{
			velocity.Y = JumpVelocity;
		}

		if (Input.IsActionJustPressed("dodge"))
		{
			velocity = new Vector3(velocity.X * this.DodgeMultiplier, velocity.Y, velocity.Z * this.DodgeMultiplier);
		}

		Velocity = velocity.Lerp(Direction * MoveSpeed, Acceleration * ((float)delta));
		MoveAndSlide();
	}
}
