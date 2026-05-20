using System.Text.Json.Serialization;

namespace SpiceSharpParserAIExamples;

internal sealed class AcceptedExampleCase
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("source")]
    public string Source { get; init; } = "";

    [JsonPropertyName("example_id")]
    public string ExampleId { get; init; } = "";

    [JsonPropertyName("test_name")]
    public string TestName { get; init; } = "";

    [JsonPropertyName("netlist_hash")]
    public string NetlistHash { get; init; } = "";

    [JsonPropertyName("generator_model")]
    public string GeneratorModel { get; init; } = "";

    [JsonPropertyName("generator_source")]
    public string GeneratorSource { get; init; } = "";

    [JsonPropertyName("quality_tier")]
    public string QualityTier { get; init; } = "";

    [JsonPropertyName("status")]
    public string Status { get; init; } = "";

    [JsonPropertyName("pytest_status")]
    public string PytestStatus { get; init; } = "";

    [JsonPropertyName("prompt_count")]
    public int PromptCount { get; init; }

    [JsonPropertyName("representative_prompt")]
    public string RepresentativePrompt { get; init; } = "";

    [JsonPropertyName("pair_ids")]
    public List<string> PairIds { get; init; } = [];

    [JsonPropertyName("prompts")]
    public List<string> Prompts { get; init; } = [];

    [JsonPropertyName("measurements")]
    public List<MeasurementReport> Measurements { get; init; } = [];
}

internal sealed class MeasurementReport
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("key")]
    public string Key { get; init; } = "";

    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("value")]
    public double? Value { get; init; }

    [JsonPropertyName("value_is_finite")]
    public bool ValueIsFinite { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("measurement_type")]
    public string MeasurementType { get; init; } = "";

    [JsonPropertyName("simulation_name")]
    public string SimulationName { get; init; } = "";
}

internal sealed class AcceptedExamplesManifest
{
    [JsonPropertyName("total_unique_cases")]
    public int TotalUniqueCases { get; init; }

    [JsonPropertyName("unmatched_accepted_pair_hashes")]
    public int UnmatchedAcceptedPairHashes { get; init; }

    [JsonPropertyName("sources")]
    public List<AcceptedExamplesSourceManifest> Sources { get; init; } = [];
}

internal sealed class AcceptedExamplesSourceManifest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("accepted_pairs_path")]
    public string AcceptedPairsPath { get; init; } = "";

    [JsonPropertyName("accepted_pairs_sha256")]
    public string AcceptedPairsSha256 { get; init; } = "";

    [JsonPropertyName("accepted_pair_count")]
    public int AcceptedPairCount { get; init; }

    [JsonPropertyName("unique_netlist_count")]
    public int UniqueNetlistCount { get; init; }

    [JsonPropertyName("measured_examples_path")]
    public string MeasuredExamplesPath { get; init; } = "";

    [JsonPropertyName("measured_examples_sha256")]
    public string MeasuredExamplesSha256 { get; init; } = "";

    [JsonPropertyName("measured_example_count")]
    public int MeasuredExampleCount { get; init; }
}
