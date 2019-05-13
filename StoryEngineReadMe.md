#Story Engine

## How the code works

The Story Engine uses the metaphor of a theatre to organise the code and create flows. A director executes a script, and an assistant director distributes tasks to various handlers. Every handler receives the task and must sign off on it. If nothing is specified, it automatically signs off. Note that functionality can live anywhere, the handlers are intended for conceptual organisation of code. 

###Handlers
Current handlers are: 

|handler|description|
---|---|
|set handler | runs everything that is part of the visual stage |
|user handler | takes care of user interaction and feedback |
|data handler| runs networking and other data tasks |
|agent handler| handles any (pro-)active elements - this isn't used in jim engine |
| deus handler | used for development purposes such as debugging and storyboarding |


###Script

The script is organised into storylines. The tasks are distributed to all handlers, on all connected devices (if the task is global, see below). Every handler and every device works on the same task object and the same storylines.

\#SOMESTORY  
TASK  
ANOTHERTASK  
END  

It is possible to control other storylines from the script, creating more complex flows.

START THATSTORY  
STOP THATSTORY  
TELL THATSTORY MARKER

where

\#THATSTORY   
TASK    
@MARKER  
ANOTHERTASK    
END  

A task may perform a callback, which results in starting a storyline from the script. 

###UI

The story engine also provides a set of UI functionality for creating UI elements and allowing users to interact with them. A button can perform a callback, launching a storyline from the script. Button interaction can be organised by using drag constraints and drag targets (eg. to drag an entire menu). Updating the UI is loop based, via a script call on every frame.

###Networking

The story engine provides networking functionality such as network discovery, server-client connections, syncing global tasks and storylines across the network and load balancing.


###Global vs Local
Tasks and storylines can be global (synced across the network) or local. Global tasks share the same variables and can trigger callbacks across the network. The server handles global events such as callback and progressing the script. When a client connects it receives the current state of all global tasks and storylines. When a client disconnects global tasks and storylines revert to local ones, so the client can keep running. Devices do not know if another device has finished a task or not.



