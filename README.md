# NightFall Authentication Server

Authentication Server for NightFall game made with Godot using Mono.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

If you want to export the project yourself, you need my fork of Godot Engine. Otherwise go straight to [Installing](#Installing) section.

```
git clone https://github.com/NeqsTech/godot -b neqstech-fork
```

Then follow the prerequisites from https://github.com/NeqsTech/godot/tree/neqstech-fork to get an editor or export template.

### Installing

Generate 2 diffrent strings of random characters (I will call them tokens).

Use those tokens to set 2 following environment variables:

```
$ export GATEWAY_TOKEN one_of_the_tokens
$ export GAME_SERVER_TOKEN the_other_token
```

Same tokens have to be used for [NightFall Gateway Server](https://github.com/NeqsTech/nightfall-gateway-server) and [NightFall GameWorld Server](https://github.com/NeqsTech/nightfall-gameworld-server) projects.

You also need to generate self-signed certifiacte and a private key.

```
$ openssl req -new -newkey rsa:4096 -nodes -x509 -keyout basic.key -out basic.crt
```

Put `basic.key` and `basic.crt` at `user://`. On linux it's `~/.local/share/godot/app_userdata/NightFall Authentication Server/DTLS/`.

Same as before you need to put those files at `user://` for both [NightFall Gateway Server](https://github.com/NeqsTech/nightfall-gateway-server) and [NightFall GameWorld Server](https://github.com/NeqsTech/nightfall-gameworld-server). The specific instructions can be found at: TODO


## Deployment

This application will be run on K8s cluster in the future.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.


