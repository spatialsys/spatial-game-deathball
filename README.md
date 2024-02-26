# For Regression:
As of writing the game is an extrememly minimal prototype of the gameplay. A ball bounces between players/bots and checks for a block state each hit.

In this version bots are a [Synced Object](https://docs.spatial.io/components/synced-object) which are controlled by a single client at a time.

The game can handle multiple players and players joining/leaving.

> [!WARNING]
> As of writing actor custom properties are not properly synced resulting in non-host clients being unable to block the ball. Fix coming next Spatial.io patch.

## GOTO
`Bot.cs` to control the bots and do ai stuff...

List of bots available in `BotManager.cs`

`BallControl.cs` to modify how the ball moves and what it does on collision

`LocalPlayerControls.cs` to modify what the local player can do.
