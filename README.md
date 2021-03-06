# Bantam
A lightweight, no garbage application framework for C# that's compatible with Unity.

[![Build Status](https://travis-ci.org/wlindley/bantam.svg?branch=master)](https://travis-ci.org/wlindley/bantam)

## Features
1. **Object Pool** can be used with generic type parameters or Type object parameters. Pools expose the number of instances of each type they have created to aid with debugging and profiling.
1. **Event Bus** ensures all Events are pooled while still allowing custom data to be assigned to each instance. Supports adding/removing Event listeners, listeners that only get called once, and listeners that will receive all Events on the bus.
1. **Command Relay** will execute sequential chains of Commands in response to Events or individually. Multiple Command chains can be registered for an Event, in which case they will run in parallel. Commands are synchronous by default, but can be made asynchronous on demand. If a Command fails, subsequent Commands in the chain will not be executed.
1. **Model Registry** will manage a pool of active Models and let you retrieve them by type. Also emits events when Models are created and deleted, making it easy to write a wrapper for whatever GUI system you're using to bind views to Models.

## Examples
```csharp
//Allocate and free an object.
var doc = pool.Allocate<Document>();
pool.Free<Document>(doc);

//Register an event listener.
eventBus.AddListener<LoginEvent>(evt => server.Login(evt.username, evt.password));

//Dispatch an event.
eventBus.Dispatch<LoginEvent>(evt => {
	evt.username = "user_one";
	evt.password = hashedPassword;
});

//Bind commands to an event.
commandRelay.On<LoginEvent>()
	.Do<ShowUsernameCommand>((cmd, evt) => cmd.loginEvent = evt)
	.Do<ShowOptionsCommand>();

//Handle a failed command chain
commandRelay.On<UserDataRequestEvent>()
	.Do<FailingCommand>()
	.OnFailure<ResetUserDataCommand>();

//Create a Model.
modelRegistry.Create<UserDataModel>(mdl => mdl.username = "Jane Doe");
```
