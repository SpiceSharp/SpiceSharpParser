namespace SpiceSharpParser.ModelWriters.CSharp
{
    public enum CSharpStatementKind
    {
        CreateEntity,
        CreateSimulation,
        CreateSimulationInit_After,
        CreateSimulationInit_Before,
        SetSimulation,
        OtherSimulation,
        Configuration,
    }
}
