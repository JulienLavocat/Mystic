#if TOOLS
using Godot;

namespace MysticFramework;

[Tool]
public partial class Plugin : EditorPlugin
{

	private const string RunBarPath = "res://addons/MysticFramework/Tools/RunBar.tscn";

	private Control _runbar;
	
	public override void _EnterTree()
	{
		_runbar = (Control)GD.Load<PackedScene>("res://addons/MysticFramework/Tools/RunBar.tscn").Instantiate<Node>();
		AddChild(_runbar);
		_runbar.Hide();
	}
	
	public override void _ExitTree()
	{
		_runbar.QueueFree();
	}
	
}
#endif
