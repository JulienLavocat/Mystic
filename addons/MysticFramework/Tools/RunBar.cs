#if TOOLS
using Godot;
using System.Collections.Generic;
using System.Threading;
using Godot.Collections;

namespace MysticFramework.Tools;

[Tool]
public partial class RunBar : Control
{
	private const string RunBarThemeType = "RunBarButton";
	private const string ClientSleepTimeSetting = "mystic/run/client_sleep_time";

	private Texture2D _playProjectIcon;
	private Texture2D _playSceneIcon;
	private Button _playProjectBtn;
	private Button _playSceneBtn;
	private Button _mysticPlayBtn;

	private ServerExporter _exporter = new();
	private Window _window;

	public override void _EnterTree()
	{
		AddProjectSettings();
		
		_exporter.OnBegin += () => GD.Print("Exporting server");
		_exporter.OnSuccess += OnExportSuccess;
		_exporter.OnError += OnExportError;
		_exporter.OnProgress += (line, _) => GD.Print(line);
		
		var editorInterface = EditorInterface.Singleton.GetBaseControl();
		_playProjectIcon = editorInterface.GetThemeIcon("MainPlay", "EditorIcons");
		_playSceneIcon = editorInterface.GetThemeIcon("PlayScene", "EditorIcons");

		foreach (var runBarButton in GetRunBarButtons(EditorInterface.Singleton.GetBaseControl()))
		{
			if (runBarButton.Icon == _playSceneIcon) _playSceneBtn = runBarButton;
			if (runBarButton.Icon == _playProjectIcon) _playProjectBtn = runBarButton;
		}

		if (_playProjectBtn == null)
		{
			GD.PushError("Play project button not found");
			return;
		}

		if (_playSceneBtn == null) GD.PushWarning("Play scene button not found");

		_playProjectBtn.Hide();
		_playSceneBtn?.Hide();

		_mysticPlayBtn = new Button();
		_mysticPlayBtn.SetThemeTypeVariation(RunBarThemeType);
		_mysticPlayBtn.SetToggleMode(true);
		_mysticPlayBtn.SetFocusMode(Control.FocusModeEnum.None);
		_mysticPlayBtn.SetTooltipText("Export the server and then run the project.");
		_mysticPlayBtn.SetButtonIcon(_playProjectIcon);
		_mysticPlayBtn.SetShortcutInTooltip(true);
		if (_playProjectBtn != null)
			_mysticPlayBtn.SetShortcut(_playProjectBtn.Shortcut);
		_mysticPlayBtn.Pressed += RunProject;

		_playProjectBtn.GetParent().AddChild(_mysticPlayBtn);
		_playProjectBtn.GetParent().MoveChild(_mysticPlayBtn, 1);
	}

	public override void _ExitTree()
	{
		_playSceneBtn?.Show();
		_playProjectBtn?.Show();

		if (_mysticPlayBtn == null) return;
		_mysticPlayBtn.Pressed -= RunProject;
		_mysticPlayBtn.QueueFree();
	}

	private void RunProject()
	{
		// TODO: Add progress bar
		_exporter.Start();
	}

	private void OnExportSuccess()
	{
		GD.Print("Server export completed");
		var sleepTime = ProjectSettings.GetSetting(ClientSleepTimeSetting, 1500).AsInt32();
		GD.Print($"Waiting {sleepTime / 1000}s for server to be ready...");
		Thread.Sleep(sleepTime);
		EditorInterface.Singleton.CallDeferred(EditorInterface.MethodName.PlayMainScene);
	}

	private void OnExportError(string error)
	{
		GD.PushError(error);
		GD.Print("Server export error");
	}

	private void AddProjectSettings()
	{
		if (ProjectSettings.HasSetting(ClientSleepTimeSetting)) return;
		
		ProjectSettings.AddPropertyInfo(new Dictionary()
		{
			{"name", "mystic/run/client_sleep_time"},
			{"type", (int) Variant.Type.Int},
			{"hint", (int) PropertyHint.None },
			{"hint_string", "1000, 1500, 10000"}
		});
		
		ProjectSettings.SetSetting(ClientSleepTimeSetting, 1500);
			
	}

	/**
	 * Finds all buttons having a "RunBarButton" theme type attached to them recursively across all child nodes.
	 * If it stops working after an engine update, see editor_run_bar.cpp: https://github.com/godotengine/godot/blob/master/editor/gui/editor_run_bar.cpp#L365
	*/
	private static List<Button> GetRunBarButtons(Node node)
	{
		var buttons = new List<Button>();
		foreach (var child in node.GetChildren())
		{
			if (child is Button btn && btn.GetThemeTypeVariation() == RunBarThemeType) buttons.Add(btn);
			else
				buttons.AddRange(GetRunBarButtons(child));
		}

		return buttons;
	}
}
#endif