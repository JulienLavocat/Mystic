docker_build("localhost:5005/mystic/gs", "", ignore=["./Client", "./local"])

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
    k8s_resource(workload=gs_id, port_forwards="%d:9050" % port)

launch_game_server(0)
launch_game_server(1)
launch_game_server(2)
launch_game_server(4)
