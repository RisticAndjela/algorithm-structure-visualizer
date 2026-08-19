namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.DecisionTree;

public enum DecisionTreeCriterion { Gini, Entropy }
public enum DecisionTreePhase { Ready, InspectingNode, EvaluatingSplit, Splitting, Complete }
public enum DecisionTreeNodeState { Stored, Active, Split, Leaf }

public sealed record DecisionTreeConfiguration(
    double[][] Features,
    int[] Labels,
    DecisionTreeCriterion Criterion = DecisionTreeCriterion.Gini,
    int MaxDepth = 3,
    double MinimumGain = 1e-9d);

public sealed record DecisionTreeNodeSnapshot(
    int Id,
    int Depth,
    int[] ExampleIndices,
    int ZeroCount,
    int OneCount,
    int Prediction,
    double Impurity,
    bool IsLeaf,
    int SplitFeature,
    double SplitThreshold,
    double Gain,
    int LeftId,
    int RightId,
    DecisionTreeNodeState State);

public sealed record DecisionTreeSnapshot(
    double[][] Features,
    int[] Labels,
    DecisionTreeNodeSnapshot[] Nodes,
    DecisionTreeCriterion Criterion,
    DecisionTreePhase Phase,
    int CurrentNodeId,
    int CandidateFeature,
    double CandidateThreshold,
    double CandidateGain,
    int CandidateLeftCount,
    int CandidateRightCount,
    string FocusText)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
}

public sealed record DecisionTreeRunResult(
    double[][] Features,
    int[] Labels,
    DecisionTreeNodeSnapshot[] Nodes,
    DecisionTreeCriterion Criterion,
    int RootFeature,
    double RootThreshold,
    double RootGain,
    int TreeDepth,
    int LeafCount,
    int CorrectPredictions,
    string Summary)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
    public double TrainingAccuracy => Count == 0 ? 0d : CorrectPredictions / (double)Count;
    public string BuildComplexity => "O(n·f·s) teaching scan";
    public string PredictionComplexity => "O(depth)";
}
