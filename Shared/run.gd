extends Node


func _ready() -> void:
	var args: PackedStringArray = OS.get_cmdline_args()
	print(args)
	if args.find("server") > 0:
		get_tree().call_deferred("change_scene_to_file", "res://Server/Main.tscn")
		return
		
	get_tree().call_deferred("change_scene_to_file", "res://Client/Demo/Demo.tscn")
