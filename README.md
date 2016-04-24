# bantam
A lightweight application framework for C#.

## Features
1. **Object pool** that can be used with generic type parameters or Type object parameters. It also exposes the number of instances of each type it has created to aid with debugging and profiling.
1. **Event bus** that ensures all Events are pooled while still allowing custom data to be assigned to each instance. Supports adding/removing Event listeners, listeners that only get called once, and listeners that will receive all Events on the bus.
1. **Command relay** that will execute sequential chains of Commands in response to Events. Multiple Command chains can be registered for an Event, in which case they will run in parallel. Commands are synchronous by default, but can be made asynchronous on demand. If a Command fails, subsequent Commands in the chain will not be executed.
