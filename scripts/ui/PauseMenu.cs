using Godot;

public partial class PauseMenu : CanvasLayer
{
    public override void _Ready()
    {
        // Must be Always so this node receives input while the tree is paused
        ProcessMode = ProcessModeEnum.Always;

        Hide();

        GetNode<Button>("Panel/VBoxContainer/LeaveButton")
            .Pressed += OnLeavePressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePause();
            GetViewport().SetInputAsHandled();
        }
    }

    private void TogglePause()
    {
        if (Visible)
        {
            Hide();
            GetTree().Paused = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Show();
            GetTree().Paused = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    private void OnLeavePressed()
    {
        GetTree().Paused = false;
        GetTree().Quit();
    }
}