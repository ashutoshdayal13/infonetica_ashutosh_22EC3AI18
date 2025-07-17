using System.Collections.Concurrent;
using WorkflowEngineApi.Models;

namespace WorkflowEngineApi.Services;

public class WorkflowEngine
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
    private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new();

    public WorkflowDefinition CreateDefinition(WorkflowDefinition definition)
    {
        ValidateDefinition(definition);
        if (!_definitions.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Workflow definition with id {definition.Id} already exists");
        }
        return definition;
    }

    public WorkflowDefinition GetDefinition(string id)
    {
        return _definitions.TryGetValue(id, out var def)
            ? def
            : throw new KeyNotFoundException($"Workflow definition {id} not found");
    }

    public WorkflowInstance StartInstance(string definitionId)
    {
        var def = GetDefinition(definitionId);
        var initial = def.States.Single(s => s.IsInitial);
        var instance = new WorkflowInstance
        {
            DefinitionId = def.Id,
            CurrentState = initial.Id
        };
        _instances[instance.Id] = instance;
        return instance;
    }

    public WorkflowInstance GetInstance(string id)
    {
        return _instances.TryGetValue(id, out var inst)
            ? inst
            : throw new KeyNotFoundException($"Instance {id} not found");
    }

    public WorkflowInstance ExecuteAction(string instanceId, string actionId)
    {
        var instance = GetInstance(instanceId);
        var definition = GetDefinition(instance.DefinitionId);
        var action = definition.Actions.SingleOrDefault(a => a.Id == actionId)
            ?? throw new InvalidOperationException($"Action {actionId} not found in workflow definition");

        if (!action.Enabled)
            throw new InvalidOperationException($"Action {actionId} is disabled");

        var current = definition.States.Single(s => s.Id == instance.CurrentState);
        if (current.IsFinal)
            throw new InvalidOperationException("Instance is already in a final state");

        if (!action.FromStates.Contains(current.Id))
            throw new InvalidOperationException($"Action {actionId} cannot be executed from state {current.Id}");

        if (!definition.States.Any(s => s.Id == action.ToState))
            throw new InvalidOperationException($"Target state {action.ToState} does not exist");

        instance.CurrentState = action.ToState;
        instance.History.Add(new InstanceHistoryEntry(actionId, DateTime.UtcNow));
        return instance;
    }

    private static void ValidateDefinition(WorkflowDefinition definition)
    {
        if (definition.States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicate state IDs are not allowed");
        if (definition.Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicate action IDs are not allowed");
        if (definition.States.Count(s => s.IsInitial) != 1)
            throw new InvalidOperationException("Exactly one initial state must be defined");
        var stateIds = definition.States.Select(s => s.Id).ToHashSet();
        foreach (var action in definition.Actions)
        {
            if (!stateIds.Contains(action.ToState))
                throw new InvalidOperationException($"Action {action.Id} has unknown toState {action.ToState}");
            foreach (var from in action.FromStates)
            {
                if (!stateIds.Contains(from))
                    throw new InvalidOperationException($"Action {action.Id} references unknown fromState {from}");
            }
        }
    }
}
