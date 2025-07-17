namespace WorkflowEngineApi.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<State> States { get; set; } = new();
    public List<WorkflowAction> Actions { get; set; } = new();
}
