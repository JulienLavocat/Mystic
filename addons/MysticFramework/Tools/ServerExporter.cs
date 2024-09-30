using System;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace MysticFramework.Tools;

public class ServerExporter
{
	public event Action OnBegin;
	public event Action OnSuccess;
	public event Action<string> OnError;
	public event Action<string, int> OnProgress;

	public int TotalSavePacksSteps { get; private set; }

	private readonly Process _process;

	public ServerExporter()
	{
		_process = new Process();
		_process.StartInfo.FileName = OS.GetExecutablePath();
		_process.StartInfo.Arguments =
			"--headless --export-debug \"Server Debug\" \"builds/server/server.x86_64\"";
		_process.StartInfo.RedirectStandardError = true;
		_process.StartInfo.RedirectStandardOutput = true;
		_process.EnableRaisingEvents = true;

		_process.Exited += OnExit;
		_process.OutputDataReceived += EmitProgress;
	}

	public void Start()
	{
		_process.Start();
		_process.BeginOutputReadLine();
		OnBegin?.Invoke();
	}

	private void OnExit(object sender, EventArgs args)
	{
		_process.CancelOutputRead();
		if (_process.ExitCode > 0)
			OnError?.Invoke(_process.StandardError.ReadToEnd());
		else
			OnSuccess?.Invoke();
	}

	private void EmitProgress(object sender, DataReceivedEventArgs args)
	{
		if (args.Data == null) return;
		var line = args.Data.Trim();

		if (line.StartsWith("dotnet_publish_project:"))
			OnProgress?.Invoke(line, 0);

		if (line.StartsWith("savepack: begin"))
		{
			TotalSavePacksSteps = int.Parse(line.Split(" ").Last());
			OnProgress?.Invoke(line, 0);
		}

		if (line.StartsWith("savepack: step"))
		{
			var step = int.Parse(line.Split(" ")[2].Trim(':'));
			OnProgress?.Invoke(line, step);
		}
	}
}