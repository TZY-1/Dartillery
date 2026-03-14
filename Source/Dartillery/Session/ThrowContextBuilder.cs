using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Builds ThrowContext objects by coordinating various behavioral models (tremor, pressure, momentum).
/// Responsible for aggregating modifiers from different sources into a single context for throw execution.
/// </summary>
/// <remarks>
/// This class encapsulates the logic for calculating and combining effects from:
/// - Tremor models (fatigue over time)
/// - Pressure models (psychological effects in high-stakes situations)
/// - Momentum models (hot/cold streaks)
/// - Grouping models (dart clustering effects)
/// - Target difficulty models (difficulty variations by target type)
/// </remarks>
public sealed class ThrowContextBuilder
{
    private readonly PlayerProfile _profile;
    private readonly ITremorModel _tremorModel;
    private readonly IPressureModel _pressureModel;
    private readonly IMomentumModel _momentumModel;
    private readonly IGroupingModel _groupingModel;
    private readonly ITargetDifficultyModel _targetDifficultyModel;

    private double _currentTremor = 0.0;

    /// <summary>
    /// Initializes a new throw context builder with the specified behavioral models.
    /// </summary>
    /// <param name="profile">The player profile for base characteristics.</param>
    /// <param name="tremorModel">Model for calculating tremor/fatigue effects.</param>
    /// <param name="pressureModel">Model for calculating pressure/psychological effects.</param>
    /// <param name="momentumModel">Model for calculating momentum/streak effects.</param>
    /// <param name="groupingModel">Model for calculating dart clustering effects.</param>
    /// <param name="targetDifficultyModel">Model for calculating target-specific difficulty.</param>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public ThrowContextBuilder(
        PlayerProfile profile,
        ITremorModel tremorModel,
        IPressureModel pressureModel,
        IMomentumModel momentumModel,
        IGroupingModel groupingModel,
        ITargetDifficultyModel targetDifficultyModel)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        ArgumentNullException.ThrowIfNull(tremorModel);
        ArgumentNullException.ThrowIfNull(pressureModel);
        ArgumentNullException.ThrowIfNull(momentumModel);
        ArgumentNullException.ThrowIfNull(groupingModel);
        ArgumentNullException.ThrowIfNull(targetDifficultyModel);
        _tremorModel = tremorModel;
        _pressureModel = pressureModel;
        _momentumModel = momentumModel;
        _groupingModel = groupingModel;
        _targetDifficultyModel = targetDifficultyModel;
    }

    /// <summary>
    /// Gets the current tremor magnitude calculated by the tremor model.
    /// </summary>
    /// <remarks>
    /// This value accumulates over the session based on the tremor model's algorithm
    /// and is reset when the session is reset.
    /// </remarks>
    public double CurrentTremor => _currentTremor;

    /// <summary>
    /// Builds a complete ThrowContext by evaluating all behavioral models.
    /// </summary>
    /// <param name="sessionState">Current session state including throw count and timing.</param>
    /// <param name="throwHistory">Complete history of throws for momentum/grouping calculations.</param>
    /// <param name="gameContext">Optional game context for pressure calculations (e.g., remaining score).</param>
    /// <returns>A fully populated ThrowContext ready for throw execution.</returns>
    /// <remarks>
    /// This method orchestrates the following steps:
    /// 1. Calculate current tremor based on session state
    /// 2. Calculate pressure modifier based on game situation
    /// 3. Calculate momentum modifier based on recent performance
    /// 4. Extract previous throws in current visit from game context
    /// 5. Combine all modifiers into a ThrowContext
    /// </remarks>
    public ThrowContext BuildContext(
        SessionState sessionState,
        IReadOnlyList<ThrowResult> throwHistory,
        GameContext? gameContext = null)
    {
        // Update tremor based on session progress
        var stateWithTremor = sessionState with { CurrentTremor = _currentTremor };
        _currentTremor = _tremorModel.CalculateTremor(stateWithTremor, _profile);

        // Calculate pressure modifier (>= 1.0, where higher = more pressure = worse performance)
        double pressureModifier = gameContext != null
            ? _pressureModel.GetPrecisionModifier(_profile, gameContext)
            : 1.0;

        // Calculate momentum modifier (can be < 1.0 for hot streaks, > 1.0 for cold streaks)
        double momentumModifier = _momentumModel.CalculateMomentumModifier(throwHistory);

        // Extract previous throws in current visit for grouping calculations
        var previousThrowsInVisit = gameContext?.CurrentVisitResults ?? new List<ThrowResult>();

        return new ThrowContext
        {
            SessionTremor = _currentTremor,
            PressureModifier = pressureModifier,
            MomentumModifier = momentumModifier,
            ThrowIndexInSession = sessionState.ThrowCount,
            GameContext = gameContext,
            PreviousThrowsInVisit = previousThrowsInVisit
        };
    }

    /// <summary>
    /// Resets the tremor state to zero.
    /// </summary>
    /// <remarks>
    /// Called when a session is reset to clear accumulated fatigue.
    /// Other models are stateless and don't require reset.
    /// </remarks>
    public void Reset()
    {
        _currentTremor = 0.0;
    }
}
