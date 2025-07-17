# Workflow Engine API

This repository contains a minimal ASP.NET Core 8.0 web API implementing a simple configurable workflow engine/state machine.

## Quick start

1. Ensure the .NET 8 SDK is installed.
2. Run the service:

```bash
dotnet run --project WorkflowEngineApi
```

The API listens on `http://localhost:5000` by default.

## Sample requests

### Create a workflow definition

```http
POST /workflows
Content-Type: application/json

{
  "id": "simple",
  "states": [
    { "id": "start", "name": "Start", "isInitial": true },
    { "id": "done", "name": "Done", "isFinal": true }
  ],
  "actions": [
    {
      "id": "finish",
      "name": "Finish",
      "fromStates": ["start"],
      "toState": "done"
    }
  ]
}
```

### Start a new instance

```http
POST /workflows/simple/instances
```

### Execute an action

```http
POST /instances/{instanceId}/actions/finish
```

### Retrieve instance status

```http
GET /instances/{instanceId}
```

## Assumptions & Limitations

- Persistence is in-memory only. Restarting the service clears all data.
- Validation errors return `400 Bad Request` with a message.
- This project focuses on core functionality and omits authentication, logging, and extensive testing.
