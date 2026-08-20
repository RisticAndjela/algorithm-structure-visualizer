# Run once from the repository root after extracting this patch over the project.
# The new Phase 2 files are already in place; this only removes the retired standalone Computational Graph lesson.
$ErrorActionPreference = 'Stop'

$paths = @(
  'src/AlgorithmVisualizer.Core/MachineLearning/DeepLearning/ComputationalGraph/ComputationalGraphModels.cs',
  'src/AlgorithmVisualizer.Core/MachineLearning/DeepLearning/ComputationalGraph/ComputationalGraphSimulation.cs',
  'src/AlgorithmVisualizer.Core/MachineLearning/DeepLearning/ComputationalGraph/README.md',
  'src/AlgorithmVisualizer.Client/Components/Visualization/ComputationalGraph/ComputationalGraphPlot.razor',
  'src/AlgorithmVisualizer.Client/Components/Visualization/ComputationalGraph/ComputationalGraphPlot.razor.css',
  'src/AlgorithmVisualizer.Client/Pages/MachineLearning/ComputationalGraphPage.razor',
  'src/AlgorithmVisualizer.Client/Pages/MachineLearning/ComputationalGraphPage.razor.css',
  'tests/AlgorithmVisualizer.Core.Tests/MachineLearning/DeepLearning/ComputationalGraphSimulationTests.cs'
)

foreach ($path in $paths) {
  if (Test-Path -LiteralPath $path) {
    Remove-Item -LiteralPath $path -Force
    Write-Host "Removed $path"
  }
}

$emptyDirs = @(
  'src/AlgorithmVisualizer.Core/MachineLearning/DeepLearning/ComputationalGraph',
  'src/AlgorithmVisualizer.Client/Components/Visualization/ComputationalGraph'
)
foreach ($dir in $emptyDirs) {
  if (Test-Path -LiteralPath $dir) {
    $remaining = @(Get-ChildItem -LiteralPath $dir -Force)
    if ($remaining.Count -eq 0) { Remove-Item -LiteralPath $dir -Force }
  }
}

Write-Host 'Phase 2 patch cleanup complete.'
