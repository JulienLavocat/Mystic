# Mystic

Mystic is an MMO game framework for Godot 4+.

## Local development

### Introduction

In order to work on Mystic and iterate quickly, you will need to have these components running locally:

- At least 1 game server
- At least 1 client
- (Not yet implemented) the whole API services (Database, etc.)

Running these manually can be a huge hassle, especially having to restart the game servers everytime you modify
something, debugging is also an issue as you will need a centralized way to see all the servers logs.

In order to solve these issues, Mystic will create a **local** [Kubernetes](https://kubernetes.io/) cluster with support
of hot-reload of the servers using [Tilt](https://docs.tilt.dev). Each changes **related** to the server will trigger a
rebuild and restart, you'll just have to wait for completion before starting the client.

The following sections will first walk you through the Godot setup (engine version and addons
using [GodotEnv](https://github.com/chickensoft-games/GodotEnv/)) and then you'll setup the Kubernetes cluster, API (Not
yet implemented) and gameservers in a single command.

### Setup

#### Requirements

Before working on a Mystic project, you'll need to install the following tools:

1. Install [cptl](https://github.com/tilt-dev/ctlptl) (used to manage the Kubernetes cluster initialisation)
2. Install [Kind](https://kind.sigs.k8s.io) (used to setup a local kubernetes cluster)
3. Install [Tilt](https://docs.tilt.dev/) (used to hot-reload the servers)
4. Install [GodotEnv](https://github.com/chickensoft-games/GodotEnv/) (used to manage Godot versions and addons)
5. Install Make using the prefered method for your OS

#### Setup steps

1. Clone the repository
2. Run `make godot` to setup your Godot version and addons using Godotenv
3. **[ONLY ONCE]** Run `make dns`, to add `monitoring.mystic.local` to your host file
4. Run `make cluster` to setup the Kubernetes cluster and have the monitoring tools deployed
5. Run `tilt up` and press space while the process is running to open the web ui (or go
   to http://localhost:10350/overview)

### Developing your game using Mystic

Once the setup steps are completed, you are now ready to work on your game. This section will describe how to work
with Mystic as your framework and give some examples to implement some features (TODO!)

#### Inside the Godot editor

TODO: Add godot side of things once done, also add documentation links

#### Registering, sending and receiving a packet

The end goal is to have a custom protocol file format to easily generate the packets, for now everything is registered
manually in your code.

##### Registering a packet

Each packets are registered automatically when subscribed / sent, you only need to register it's nested types if they
aren't on of these types :

```
byte sbyte short ushort int uint long ulong float double bool string char IPEndPoint
```

To do so, use `Client.RegisterNestedType<YourTypeHere>()` in the client and `Server.RegisterNestedType<YourTypeHere>()`
in
in the server **BEFORE** using your packet (otherwise all it's fields will be uninitialized).

Example :

```csharp
// Server
Server.RegisterNestedType<PlayerInput>();
Server.Subscribe<PlayerInputPacket>(HandlePlayerInput);

// Client
Client.RegisterNestedType<PlayerInput>();
Client.Send(new PlayerInput(), DeliveryMethod.Unreliable);
```

#### Servers performance and logs

To explore your servers logs,
go [here](http://monitoring.mystic.local/logs/logs-explorer?options=%7B%22selectColumns%22%3A%5B%5D%2C%22maxLines%22%3A2%2C%22format%22%3A%22raw%22%2C%22fontSize%22%3A%22large%22%7D&compositeQuery=%257B%2522queryType%2522%253A%2522builder%2522%252C%2522builder%2522%253A%257B%2522queryData%2522%253A%255B%257B%2522dataSource%2522%253A%2522logs%2522%252C%2522queryName%2522%253A%2522A%2522%252C%2522aggregateOperator%2522%253A%2522noop%2522%252C%2522aggregateAttribute%2522%253A%257B%2522key%2522%253A%2522%2522%252C%2522dataType%2522%253A%2522%2522%252C%2522type%2522%253A%2522%2522%252C%2522isColumn%2522%253Afalse%252C%2522isJSON%2522%253Afalse%257D%252C%2522timeAggregation%2522%253A%2522rate%2522%252C%2522spaceAggregation%2522%253A%2522sum%2522%252C%2522functions%2522%253A%255B%255D%252C%2522filters%2522%253A%257B%2522op%2522%253A%2522AND%2522%252C%2522items%2522%253A%255B%257B%2522key%2522%253A%257B%2522key%2522%253A%2522k8s.deployment.name%2522%252C%2522dataType%2522%253A%2522string%2522%252C%2522type%2522%253A%2522resource%2522%252C%2522isColumn%2522%253Afalse%252C%2522isJSON%2522%253Afalse%257D%252C%2522value%2522%253A%255B%2522gameserver-0%2522%255D%252C%2522op%2522%253A%2522in%2522%257D%252C%257B%2522key%2522%253A%257B%2522key%2522%253A%2522k8s.namespace.name%2522%252C%2522dataType%2522%253A%2522string%2522%252C%2522type%2522%253A%2522resource%2522%252C%2522isColumn%2522%253Afalse%252C%2522isJSON%2522%253Afalse%257D%252C%2522value%2522%253A%255B%2522default%2522%255D%252C%2522op%2522%253A%2522in%2522%257D%255D%257D%252C%2522expression%2522%253A%2522A%2522%252C%2522disabled%2522%253Afalse%252C%2522stepInterval%2522%253A60%252C%2522having%2522%253A%255B%255D%252C%2522limit%2522%253A0%252C%2522orderBy%2522%253A%255B%257B%2522columnName%2522%253A%2522timestamp%2522%252C%2522order%2522%253A%2522desc%2522%257D%255D%252C%2522groupBy%2522%253A%255B%255D%252C%2522legend%2522%253A%2522%2522%252C%2522reduceTo%2522%253A%2522avg%2522%252C%2522offset%2522%253A0%252C%2522pageSize%2522%253A0%252C%2522ShiftBy%2522%253A0%257D%255D%252C%2522queryFormulas%2522%253A%255B%255D%257D%252C%2522promql%2522%253A%255B%257B%2522name%2522%253A%2522A%2522%252C%2522query%2522%253A%2522%2522%252C%2522legend%2522%253A%2522%2522%252C%2522disabled%2522%253Afalse%257D%255D%252C%2522clickhouse_sql%2522%253A%255B%257B%2522name%2522%253A%2522A%2522%252C%2522legend%2522%253A%2522%2522%252C%2522disabled%2522%253Afalse%252C%2522query%2522%253A%2522%2522%257D%255D%252C%2522id%2522%253A%2522e2d63c4c-1aee-4290-8020-115ad97b3039%2522%257D&panelTypes=%22list%22&viewName=%22Gameservers%22&viewKey=%229583b83e-01d3-4fb5-a572-4ef91f9c67c2%22&relativeTime=15m)
To see your servers metrics (RAM/CPU usage, packets/s, bytes sent/s, etc.)
go [here](http://monitoring.mystic.local/dashboard/571d065b-52bc-47ce-8a8f-c165b3a573f7?relativeTime=5m)

## TODO

- [ ] Add a custom packet generator using a packet file definition
- [ ] Allow custom inputs in SyncCharacterBody3D (see todo comment for details) 