namespace SpiceSharpParser.ModelWriters.CSharp.Language
{
    public enum CSharpStatementKind
    {
        CreateEntity,
        CreateSimulation,
        CreateSimulationInitAfter,
        CreateSimulationInitBefore,
        SetSimulation,
        OtherSimulation,
        Configuration,
    }
}
