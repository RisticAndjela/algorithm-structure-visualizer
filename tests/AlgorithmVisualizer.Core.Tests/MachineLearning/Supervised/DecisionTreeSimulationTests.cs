using AlgorithmVisualizer.Core.MachineLearning.Supervised.DecisionTree;
using AlgorithmVisualizer.Core.Simulation;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Supervised;

public sealed class DecisionTreeSimulationTests
{
    [Fact]
    public async Task Clean_vertical_data_uses_feature_one_at_root()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[1d,1d],[1.4d,2d],[2d,1.3d],[4d,3.8d],[4.6d,4.2d],[5.2d,3.4d]],
            [0,0,0,1,1,1]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(0, result.RootFeature);
        Assert.Equal(1d, result.TrainingAccuracy, 8);
        Assert.Equal(2, result.LeafCount);
    }

    [Fact]
    public async Task Clean_horizontal_data_uses_feature_two_at_root()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[1d,1d],[2d,1.4d],[3d,1.8d],[1.4d,4d],[2.4d,4.6d],[3.4d,5.2d]],
            [0,0,0,1,1,1]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(1, result.RootFeature);
        Assert.Equal(1d, result.TrainingAccuracy, 8);
    }

    [Fact]
    public async Task Pure_dataset_stops_at_root_leaf()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[1d,1d],[2d,1.5d],[3d,2d],[4d,2.5d],[5d,3d]],
            [0,0,0,0,0]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(-1, result.RootFeature);
        Assert.Equal(1, result.LeafCount);
        Assert.Single(result.Nodes);
        Assert.True(result.Nodes[0].IsLeaf);
    }

    [Fact]
    public async Task Entropy_is_supported_as_real_split_criterion()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[1d,1d],[1.4d,2d],[2d,1.3d],[4d,3.8d],[4.6d,4.2d],[5.2d,3.4d]],
            [0,0,0,1,1,1],
            DecisionTreeCriterion.Entropy));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(DecisionTreeCriterion.Entropy, result.Criterion);
        Assert.True(result.RootGain > 0d);
    }

    [Fact]
    public async Task Prediction_follows_built_tree()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[1d,1d],[1.4d,2d],[2d,1.3d],[4d,3.8d],[4.6d,4.2d],[5.2d,3.4d]],
            [0,0,0,1,1,1]));
        await simulation.ExecuteAsync();

        Assert.Equal(0, simulation.Predict([1.6d, 1.5d]));
        Assert.Equal(1, simulation.Predict([4.8d, 3.7d]));
    }


    [Fact]
    public async Task Core_is_not_hardcoded_to_two_features()
    {
        var simulation = NewSimulation();
        simulation.Configure(new DecisionTreeConfiguration(
            [[0d,0d,1d],[0d,1d,1.2d],[1d,0d,4d],[1d,1d,4.5d]],
            [0,0,1,1]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(2, result.RootFeature);
        Assert.Equal(3, result.Dimension);
        Assert.Equal(1d, result.TrainingAccuracy, 8);
    }

    [Fact]
    public void Invalid_label_is_rejected()
    {
        var simulation = NewSimulation();
        Assert.Throws<ArgumentException>(() => simulation.Configure(new DecisionTreeConfiguration([[1d],[2d]],[0,2])));
    }

    private static DecisionTreeSimulation NewSimulation() => new(new ImmediateSimulationRuntime());
}
