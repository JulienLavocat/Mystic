load('ext://restart_process', 'docker_build_with_restart')

update_settings(max_parallel_updates=10)

docker_build_with_restart(
    "localhost:5005/mystic/gs",
    ".",
    dockerfile='local/Dockerfile',
    ignore=["./Client", "./local"],
    entrypoint="godot --headless server",
    live_update=[
        sync(".", "/app/"),
        run("cd /app && dotnet build -p:GodotTargetPlatform=linuxbsd -c Debug")
    ],
)

def launch_game_server(id):
    file = str(read_file("local/gameserver.yaml"))

    namespace = "default"
    gs_id = "gameserver-%d" % id
    port = 30000 + id

    file = file.replace("tilt_gameserver_id", gs_id)
    file = file.replace("tilt_port", str(port))
    file = file.replace("tilt_namespace", namespace)

    yaml = decode_yaml_stream(file)
    k8s_yaml(encode_yaml_stream(yaml))
    k8s_resource(workload=gs_id)

launch_game_server(0)
launch_game_server(1)
launch_game_server(2)
launch_game_server(4)
