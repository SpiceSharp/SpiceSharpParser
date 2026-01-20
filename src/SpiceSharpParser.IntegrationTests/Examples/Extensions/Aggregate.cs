using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Xml.Linq;

public class BSIM3AggregateModel : Entity
{
    /// <summary>
    /// Gets the models in the aggregate model based on sizes.
    /// </summary>
    public HashSet<BSIM3Model> Models { get; } = [];

    /// <summary>
    /// Creates an aggregate model.
    /// </summary>
    /// <param name="name">The name.</param>
    public BSIM3AggregateModel(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Creates an aggregate model based on sizes.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="models"></param>
    public BSIM3AggregateModel(string name, IEnumerable<BSIM3Model> models)
        : base(name)
    {
        foreach (var model in models)
            Models.Add(model);
    }

    public override void CreateBehaviors(ISimulation simulation)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public override IEntity Clone()
    {
        var n = new BSIM3AggregateModel(Name);
        foreach (var model in Models)
            n.Models.Add((BSIM3Model)model.Clone());
        return n;
    }
}