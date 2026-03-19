using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Aggregates tremor, pressure, momentum, and grouping modifiers from behavioral models into a <see cref="ThrowContext"/> for each throw.
/// </summary>
public sealed class ThrowContextBuilder
{
    private readonly PlayerProfile _profile;
    private readonly ITremorModel _tremorModel;
    private readonly IPressureModel _pressureModel;
    private readonly IMomentumModel _momentumModel;

#pragma warning disable S4487 // Stored for future behavioral model integration
    private readonly IGroupingModel _groupingModel;
    private readonly ITargetDifficultyModel _targetDifficultyModel;
#pragma warning restore S4487

    private double _currentTremor;

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
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(tremorModel);
        ArgumentNullException.ThrowIfNull(pressureModel);
        ArgumentNullException.ThrowIfNull(momentumModel);
        ArgumentNullException.ThrowIfNull(groupingModel);
        ArgumentNullException.ThrowIfNull(targetDifficultyModel);
        _profile = profile;
        _tremorModel = tremorModel;
        _pressureModel = pressureModel;
        _momentumModel = momentumModel;
        _groupingModel = groupingModel;
        _targetDifficultyModel = targetDifficultyModel;
    }

    /// <summary>
    /// Gets the tremor magnitude computed during the most recent <see cref="BuildContext"/> call. Resets to 0 on session reset.
    /// </summary>
    public double CurrentTremor => _currentTremor;

    /// <summary>
    /// Evaluates all behavioral models and returns a <see cref="ThrowContext"/> containing the combined modifiers for the next throw.
    /// </summary>
    /// <param name="sessionState">Current session state including throw count and timing.</param>
    /// <param name="throwHistory">Complete throw history used for momentum calculations.</param>
    /// <param name="gameContext">Optional game context for pressure calculations (e.g., remaining score).</param>
    /// <returns>A fully populated <see cref="ThrowContext"/> ready for throw execution.</returns>
    public ThrowContext BuildContext(
        SessionState sessionState,
        IReadOnlyList<ThrowResult> throwHistory,
        GameContext? gameContext = null)
    {
        ArgumentNullException.ThrowIfNull(sessionState);
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
    /// Resets accumulated tremor to zero. Call when the session is reset to clear fatigue state.
    /// </summary>
    public void Reset()
    {
        _currentTremor = 0.0;
    }
}
