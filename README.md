# Mystic

Mystic is an MMO game framework for Godot 4+.

## Local development

### Introduction

In order to work on Mystic and iterate quickly, you will need to have these components running locally:

- At least 1 game server
- At least 1 client
- (Not yet implemented) the whole API services (Database, etc.)

Running these manually can be a huge hassle, especially having to restart the game servers everytime you modify something, debugging is also an issue as you will need a centralized way to see all the servers logs.

In order to solve these issues, Mystic will create a **local** [Kubernetes](https://kubernetes.io/) cluster with support of hot-reload of the servers using [Tilt](https://docs.tilt.dev). Each changes **related** to the server will trigger a rebuild and restart, you'll just have to wait for completion before starting the client.

The following sections will first walk you through the Godot setup (engine version and addons using [GodotEnv](https://github.com/chickensoft-games/GodotEnv/)) and then you'll setup the Kubernetes cluster, API (Not yet implemented) and gameservers in a single command.

### Setup

#### Godot

1. Install [GodotEnv](https://github.com/chickensoft-games/GodotEnv/)
2. Install the correct version of Godot using `godotenv godot install 4.3.0`
3. Clone the repository
4. Initialize the addons using `godotenv addons install`

#### Servers

1. Install [cptl](https://github.com/tilt-dev/ctlptl), [Kind](https://kind.sigs.k8s.io)
2. Create a local image registry to publish server builds and your local kubernetes cluster `ctlptl apply -f cluster.yaml`
3. Run `tilt up` and press space while the process is running to open the web ui (or go to http://localhost:10350/overview)

## Troubleshooting

### Issues with wrong versions of addons after Install

GodotEnv has a cache system, remove the .addons folder to clear it
