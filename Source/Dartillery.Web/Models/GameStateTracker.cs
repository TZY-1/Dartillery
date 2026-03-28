using Dartillery.Core.Models;

namespace Dartillery.Web.Models;

/// <summary>
/// Tracks dynamic game state within a visit, updating remaining score,
/// checkout feasibility, and attempt counts after each dart.
/// </summary>
public sealed class GameStateTracker
{
    private readonly List<ThrowResult> _visitResults = [];
    private int _visitStartScore;

    /// <summary>
    /// The score at the start of the current scenario (set by <see cref="StartScenario"/>).
    /// </summary>
    public int ScenarioStartScore { get; private set; }

    /// <summary>
    /// Current remaining score after all recorded darts.
    /// </summary>
    public int CurrentScore { get; private set; }

    /// <summary>
    /// Darts remaining in the current 3-dart visit.
    /// </summary>
    public int DartsRemainingInVisit { get; private set; } = 3;

    /// <summary>
    /// Number of visits where a checkout was attempted but not achieved.
    /// </summary>
    public int CheckoutAttempts { get; private set; }

    /// <summary>
    /// Whether the current situation is a match-point scenario (set by scenario).
    /// </summary>
    public bool IsMatchPoint { get; private set; }

    /// <summary>
    /// Whether bust rules are enforced (score resets on bust).
    /// </summary>
    public bool EnforceBustRules { get; set; } = true;

    /// <summary>
    /// Whether the last recorded dart caused a bust.
    /// </summary>
    public bool LastDartWasBust { get; private set; }

    /// <summary>
    /// Results from the current visit (up to 3 darts).
    /// </summary>
    public IReadOnlyList<ThrowResult> VisitResults => _visitResults;

    /// <summary>
    /// Whether the current score can be checked out with the remaining darts.
    /// </summary>
    public bool IsCheckoutAttempt => CheckoutTable.IsCheckable(CurrentScore, DartsRemainingInVisit);

    /// <summary>
    /// Initializes the tracker with a pressure scenario.
    /// Resets all state including checkout attempts.
    /// </summary>
    public void StartScenario(PressureScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ScenarioStartScore = scenario.RemainingScore;
        CurrentScore = scenario.RemainingScore;
        _visitStartScore = scenario.RemainingScore;
        IsMatchPoint = scenario.IsMatchPoint;
        CheckoutAttempts = scenario.CheckoutAttempts;
        DartsRemainingInVisit = 3;
        LastDartWasBust = false;
        _visitResults.Clear();
    }

    /// <summary>
    /// Builds a <see cref="GameContext"/> reflecting the current dynamic state.
    /// Called before each throw to provide accurate pressure context.
    /// </summary>
    public GameContext ToGameContext() => new()
    {
        RemainingScore = CurrentScore,
        IsCheckoutAttempt = IsCheckoutAttempt,
        IsMatchPoint = IsMatchPoint,
        CheckoutAttempts = CheckoutAttempts,
        ThrowsRemainingInVisit = DartsRemainingInVisit,
        CurrentVisitResults = _visitResults.ToList()
    };

    /// <summary>
    /// Records a throw result and updates game state accordingly.
    /// Handles score deduction, visit tracking, and bust detection.
    /// </summary>
    public void RecordThrow(ThrowResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        LastDartWasBust = false;
        _visitResults.Add(result);
        DartsRemainingInVisit--;

        int newScore = CurrentScore - result.Score;

        bool isBust = newScore < 0
                      || newScore == 1
                      || (newScore == 0 && result.SegmentType != Core.Enums.SegmentType.Double
                                        && result.SegmentType != Core.Enums.SegmentType.InnerBull);

        if (isBust && EnforceBustRules)
        {
            LastDartWasBust = true;
            CurrentScore = _visitStartScore;

            if (CheckoutTable.IsCheckable(_visitStartScore, 3))
            {
                CheckoutAttempts++;
            }

            // Force new visit
            StartNewVisit();
            return;
        }

        CurrentScore = EnforceBustRules ? newScore : Math.Max(newScore, 0);

        if (DartsRemainingInVisit <= 0)
        {
            if (CurrentScore > 0 && CheckoutTable.IsCheckable(_visitStartScore, 3))
            {
                CheckoutAttempts++;
            }

            StartNewVisit();
        }
    }

    /// <summary>
    /// Fully resets the tracker. Call when the scenario changes or throws are cleared.
    /// </summary>
    public void Reset()
    {
        ScenarioStartScore = 0;
        CurrentScore = 0;
        _visitStartScore = 0;
        DartsRemainingInVisit = 3;
        CheckoutAttempts = 0;
        IsMatchPoint = false;
        LastDartWasBust = false;
        _visitResults.Clear();
    }

    /// <summary>
    /// Resets visit state for a new 3-dart turn. Preserves score and attempts.
    /// </summary>
    private void StartNewVisit()
    {
        _visitStartScore = CurrentScore;
        DartsRemainingInVisit = 3;
        _visitResults.Clear();
    }
}
