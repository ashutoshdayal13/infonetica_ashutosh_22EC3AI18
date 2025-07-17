namespace WorkflowEngineApi.Models;

public class WorkflowInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DefinitionId { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public List<InstanceHistoryEntry> History { get; set; } = new();
}
