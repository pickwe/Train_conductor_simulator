# Advanced Event Connection Patterns

This sample demonstrates advanced usage of the UNIGINE **event system**.

*EventsAdvancedSample.cs* triggers custom rotation events when specific keys are pressed. Each event passes one or more arguments to connected listeners.

*EventsAdvancedUnit.cs* shows how to connect various types of handlers, including:

-Class methods with extra arguments
-Methods with discarded or additional arguments
-Delegates and lambda expressions
-Storing connections using *EventConnection* or an *EventConnections* instance for later disconnection

This sample helps understand flexible patterns for event handling in modular component systems.