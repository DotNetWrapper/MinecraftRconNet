# MinecraftRconNet

MinecraftRconNet is a C# (.NET Core 6) library that allows you to connect to Minecraft servers using the RCON protocol. RCON (Remote Console) enables you to send commands and manage your Minecraft server remotely. This project is a modification and derivation of ShineSmile's MineCraftServerRCON, which was originally designed for .NET Framework. It has been forked to provide compatibility with .NET Core 6. The original project is available at [ShineSmile/MineCraftServerRCON](https://github.com/ShineSmile/MineCraftServerRCON).

**Note**: This project adheres to the BSD 2-Clause "Simplified" License, and it is permissible to modify and redistribute it as long as proper credit is given to ShineSmile and their work.


## Getting Started

To use MinecraftRconNet in your C# project, follow these steps:

1. Install the Nuget "MinecraftRconNet" under https://www.nuget.org/packages/MinecraftRconNet
2. Add the usings "MinecraftRconNet"
3. Add following Code:

```csharp
using MinecraftRconNet;

using (RconClient rcon = RconClient.INSTANCE)

{
    rcon.SetupStream(host, port, password);
    string response = rcon.SendMessage(RconMessageType.Command, command);

    // Process the response as needed
}
```


# Server Setup

To set up your server, you'll need to configure some essential settings. This guide will walk you through the necessary changes to your server's configuration file.


## Prerequisites

Before you begin, make sure you have the following:

- A Minecraft server installation.
- Access to the server configuration file.

## Configuration Changes

1. **Enable RCON (Remote Console)**: RCON allows you to remotely manage your Minecraft server. To enable RCON, locate your server's configuration file (typically `server.properties`) and find the following line:

```enable-rcon=false``` and change it to ```enable-rcon=true```

2. **RCON Port**: Set the port for RCON. The default port is 25575, but you can change it if necessary. Locate the following line in your configuration file:

```rcon.port=25575``` and change it to ```rcon.port=xyz```

3. **RCON Password**: Set a secure password for RCON access. Locate the following line:

```rcon.password=WhatYouLike```


## Save and Restart

After making these changes, save the configuration file and restart your Minecraft server for the changes to take effect.

Now, you should have RCON enabled with the specified port, accessible to server operators, and secured with your chosen password. You can use RCON clients to remotely manage your Minecraft server.

Remember to keep your RCON password secure and only share it with trusted individuals who need access to your server's console.


## License

This project is licensed under the BSD 2-Clause "Simplified" License. Please see the LICENSE file for more details.


## Acknowledgments

- ShineSmile for the original [MineCraftServerRCON](https://github.com/ShineSmile/MineCraftServerRCON) project.