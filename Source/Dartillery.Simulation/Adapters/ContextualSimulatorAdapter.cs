using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Geometry;
using Dartillery.Simulation.Models.GroupingModels;
using Dartillery.Simulation.Models.TargetDifficultyModels;

namespace Dartillery.Simulation.Adapters;

/// <summary>
/// Adapter that bridges standard IThrowSimulator to contextual throws.
/// Injects context-aware deviation calculation into throw pipeline.
/// </summary>
internal sealed class ContextualSimulatorAdapter : IContextualThrowSimulator
{
    private readonly IAimPointCalculator _aimPointCalculator;
    private readonly ISegmentResolver _segmentResolver;
    private readonly IContextualDeviationCalculator _deviationCalculator;
    private readonly IGroupingModel _groupingModel;
    private readonly ITargetDifficultyModel _targetDifficultyModel;
    private readonly PlayerProfile _profile;

    public ContextualSimulatorAdapter(
        IContextualDeviationCalculator deviationCalculator,
        PlayerProfile profile,
        IGroupingModel? groupingModel = null,
        ITargetDifficultyModel? targetDifficultyModel = null,
        IAimPointCalculator? aimPointCalculator = null,
        ISegmentResolver? segmentResolver = null)
    {
        _deviationCalculator = deviationCalculator ?? throw new ArgumentNullException(nameof(deviationCalculator));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _groupingModel = groupingModel ?? new NoGroupingModel();
        _targetDifficultyModel = targetDifficultyModel ?? new NoTargetDifficultyModel();
        _aimPointCalculator = aimPointCalculator ?? new AimPointCalculator();
        _segmentResolver = segmentResolver ?? new SegmentResolver();
    }

    public ThrowResult Throw(Target target, ThrowContext context)
    {
        // Calculate base aim point
        Point2D baseAimPoint = _aimPointCalculator.CalculateAimPoint(target);

        // Apply grouping adjustment
        var (adjustedAimPoint, groupingMultiplier) = _groupingModel.AdjustForGrouping(
            baseAimPoint,
            context.PreviousThrowsInVisit);

        // Apply target difficulty
        double difficultyMultiplier = _targetDifficultyModel.GetDifficultyModifier(target);

        // Get context-aware deviation
        var (dx, dy) = _deviationCalculator.CalculateDeviation(_profile, context);

        // Apply multipliers
        dx *= groupingMultiplier * difficultyMultiplier;
        dy *= groupingMultiplier * difficultyMultiplier;

        // Calculate hit point
        Point2D hitPoint = new(adjustedAimPoint.X + dx, adjustedAimPoint.Y + dy);

        // Resolve segment (use original aim point for deviation calculation)
        return _segmentResolver.Resolve(hitPoint, baseAimPoint);
    }
}
