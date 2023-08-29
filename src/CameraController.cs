using Godot;
namespace SmoothCamera;
public partial class CameraController : Node3D
{
	[Export] public Vector2 CameraSensitivity = new Vector2(.1f, .1f);
    [Export] public float CameraHeight = 2;
    [Export] public float CameraDistance = 1;
    [Export] public Vector2 ClampAngle = new Vector2(-20, 70);
    [Export] public Vector3 Offset = Vector3.Zero;

    [Export] public float ZoomStep = .1f;
    [Export] public Vector2 ZoomLimits = new Vector2(-10, 10);
    [Export] public float ZoomLerpSpeed = 1f;
    [Export] public float CameraRotationAcel = 3f;
	[Export] public float CameraFollowAcel = 3;

	public float FacingRotation { get; set; }

    private float ZoomAmount = 0;

    private SpringArm3D SpringArm;
    private Node3D YPivot;
    private Node3D ZPivot;
    private Node3D Target;

    private float yRotation;
    private float zRotation; 
    public override void _Ready()
    { 
        this.YPivot = GetNode<Node3D>("YPivot");
        this.ZPivot = GetNode<Node3D>("YPivot/ZPivot");
        this.Target = this.ZPivot.GetNode<Node3D>("Target");

        this.SpringArm = this.GetNode<SpringArm3D>("SpringArm3D");
    }

    public override void _Input(InputEvent @event)
    { 
        if (@event is InputEventMouseMotion mm && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            yRotation = Mathf.Wrap(this.YPivot.RotationDegrees.Y - mm.Relative.X * CameraSensitivity.X, 0, 360);
            zRotation = Mathf.Clamp(this.ZPivot.RotationDegrees.X - mm.Relative.Y * CameraSensitivity.Y, ClampAngle.X, ClampAngle.Y);

            this.YPivot.RotationDegrees = new Vector3 
            (
                this.YPivot.RotationDegrees.X,
                yRotation,
                this.YPivot.RotationDegrees.Z
            );

            this.ZPivot.RotationDegrees = new Vector3
            (
                zRotation,
                this.ZPivot.RotationDegrees.Y, 
                this.ZPivot.RotationDegrees.Z
            );
        }
    }

    public override void _Process(double delta)
    {
        var playerPosition = GetParent<Player>().GlobalPosition;

        this.Target.GlobalPosition = playerPosition;
        this.GlobalPosition = playerPosition;

        var target_xform = Target.GlobalTransform.TranslatedLocal(Offset);
        var basis = this.SpringArm.GlobalTransform.Basis.Orthonormalized().Slerp(target_xform.Basis.Orthonormalized(), CameraRotationAcel * (float)delta);

        var origin = this.SpringArm.GlobalTransform.Origin;
        origin = origin.Lerp(target_xform.Origin, CameraFollowAcel * (float)delta);

        if (Target.GlobalTransform.Origin != this.SpringArm.GlobalTransform.Origin)
        {
            this.SpringArm.LookAt(Target.GlobalTransform.Origin, Target.Transform.Basis.Y);
        }

        this.SpringArm.GlobalTransform = new Transform3D(basis, origin);
        this.SpringArm.SpringLength = Mathf.Lerp(this.SpringArm.SpringLength, this.CameraDistance + this.ZoomAmount, ZoomLerpSpeed * (float) delta);

		this.FacingRotation = this.SpringArm.Rotation.Y;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        if (Input.IsActionJustPressed("zoom_in"))
        {
            this.ZoomAmount -= this.ZoomStep;
        }

        if (Input.IsActionJustPressed("zoom_out"))
        {
            this.ZoomAmount += this.ZoomStep;
        }

        this.ZoomAmount = Mathf.Clamp(this.ZoomAmount, this.ZoomLimits.X, this.ZoomLimits.Y);
    }
}
