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

    /// <summary>Initializes the adapter with deviation calculator, profile, and optional models.</summary>
    public ContextualSimulatorAdapter(
        IContextualDeviationCalculator deviationCalculator,
        PlayerProfile profile,
        IGroupingModel? groupingModel = null,
        ITargetDifficultyModel? targetDifficultyModel = null,
        IAimPointCalculator? aimPointCalculator = null,
        ISegmentResolver? segmentResolver = null)
    {
        ArgumentNullException.ThrowIfNull(deviationCalculator);
        ArgumentNullException.ThrowIfNull(profile);
        _deviationCalculator = deviationCalculator;
        _profile = profile;
        _groupingModel = groupingModel ?? new NoGroupingModel();
        _targetDifficultyModel = targetDifficultyModel ?? new NoTargetDifficultyModel();
        _aimPointCalculator = aimPointCalculator ?? new AimPointCalculator();
        _segmentResolver = segmentResolver ?? new SegmentResolver();
    }

    /// <inheritdoc/>
    public ThrowResult Throw(Target target, ThrowContext context)
    {
        Point2D aimPoint = _aimPointCalculator.CalculateAimPoint(target);
        double difficultyMultiplier = _targetDifficultyModel.GetDifficultyModifier(target);

        var (dx, dy) = _deviationCalculator.CalculateDeviation(_profile, context);
        dx *= difficultyMultiplier;
        dy *= difficultyMultiplier;

        Point2D hitPoint = new(aimPoint.X + dx, aimPoint.Y + dy);
        var deflection = _groupingModel.ApplyDeflection(hitPoint, context.PreviousThrowsInVisit);

        var result = _segmentResolver.Resolve(deflection.HitPoint, aimPoint);

        return result with
        {
            Metadata = new ThrowMetadata
            {
                FatigueMagnitude = context.SessionFatigue,
                PressureModifier = context.PressureModifier,
                MomentumModifier = context.MomentumModifier,
                WasDeflected = deflection.WasDeflected,
                DeflectionDistance = deflection.DeflectionDistance,
                PreDeflectionPoint = deflection.PreDeflectionPoint,
                DeflectedByPoint = deflection.DeflectedByPoint,
                DifficultyMultiplier = difficultyMultiplier,
                SystematicBiasApplied = _profile.SystematicBiasX,
                PlayerName = _profile.Name,
                Timestamp = DateTime.UtcNow
            }
        };
    }

    /// <inheritdoc/>
    public ThrowResult ThrowAtPoint(Point2D aimPoint, ThrowContext context)
    {
        var (dx, dy) = _deviationCalculator.CalculateDeviation(_profile, context);

        Point2D hitPoint = new(aimPoint.X + dx, aimPoint.Y + dy);
        var deflection = _groupingModel.ApplyDeflection(hitPoint, context.PreviousThrowsInVisit);

        var result = _segmentResolver.Resolve(deflection.HitPoint, aimPoint);

        return result with
        {
            Metadata = new ThrowMetadata
            {
                FatigueMagnitude = context.SessionFatigue,
                PressureModifier = context.PressureModifier,
                MomentumModifier = context.MomentumModifier,
                WasDeflected = deflection.WasDeflected,
                DeflectionDistance = deflection.DeflectionDistance,
                PreDeflectionPoint = deflection.PreDeflectionPoint,
                DeflectedByPoint = deflection.DeflectedByPoint,
                DifficultyMultiplier = 1.0,
                SystematicBiasApplied = _profile.SystematicBiasX,
                PlayerName = _profile.Name,
                Timestamp = DateTime.UtcNow
            }
        };
    }
}
