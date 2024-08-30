extends Node


func _ready() -> void:
	var args: PackedStringArray = OS.get_cmdline_args()
	if args.has("server"):
		print("Starting server")
		get_tree().call_deferred("change_scene_to_file", "res://Demo/Server/Server.tscn")
		return
	
	print("Starting client")
	get_tree().call_deferred("change_scene_to_file", "res://Demo/Client/Client.tscn")
