// Program.cs — one real house, three agents, one verdict, and a reserve that may refuse to exist.
//
// IF A LINE HERE LOOKS LIKE MATHEMATICS, IT IS IN THE WRONG FILE. The fits, the hazard and the ruin
// theory are `.hb`; the agents are `.ha`; the veto is `.hs`. This file reads the corpus, scales each
// model's answer into the integers an agent believes, collects what the agents intend, and prints.
//
// THE HOUSE IS REAL. It is a sale from houses.csv -- quality 6, 1 478 square feet, 52 years old, rated
// 3 for condition, sold for $156 500 -- found by searching the corpus for a house the three agents
// genuinely disagree about. It is asked at 10% over what it actually sold for. The LOAN is not real,
// and every line that depends on it says so.
using MLambda.AI.Actuarial;
using MLambda.Hilbert.Proof;
using MLambda.Hilbert.Runtime;

using Cond = MLambda.AI.Actuarial.ConditionAgent;
using Cred = MLambda.AI.Actuarial.CreditAgent;
using Val = MLambda.AI.Actuarial.ValuationAgent;
using Verdict = MLambda.AI.Actuarial.Adjudicator;

const double Seed = 42d;
const double ToleratedRuin = 0.05d;

Console.WriteLine("Reading 2 930 sales and fitting what the corpus says a house is worth...");

var house = new House("data/houses.csv");
var rows = house.Rows;

Matrix Design(Func<int, bool> keep, bool withCondition)
{
    var kept = Enumerable.Range(0, rows).Where(keep).ToList();
    var width = withCondition ? 5 : 4;
    var design = new double[kept.Count, width];

    for (var i = 0; i < kept.Count; i++)
    {
        var row = kept[i];
        design[i, 0] = house.Quality.Values[row];
        design[i, 1] = house.Living.Values[row];
        design[i, 2] = house.HouseAge.Values[row];

        if (withCondition)
        {
            design[i, 3] = house.Condition.Values[row];
        }

        design[i, width - 1] = 1d;
    }

    return design;
}

Vector Prices(Func<int, bool> keep) => Enumerable.Range(0, rows).Where(keep).Select(row => house.Price.Values[row]).ToArray();

// ── the subject: a real sale, found by its attributes rather than by a row number ─────────────────

var subject = Enumerable.Range(0, rows).Single(row =>
    house.Quality.Values[row] == 6 && house.Living.Values[row] == 1478 &&
    house.HouseAge.Values[row] == 52 && house.Condition.Values[row] == 3 && house.Price.Values[row] == 156500);

var quality = house.Quality.Values[subject];
var living = house.Living.Values[subject];
var age = house.HouseAge.Values[subject];
var condition = house.Condition.Values[subject];
var asking = Math.Round(house.Price.Values[subject] * 1.10);

Console.WriteLine();
Console.WriteLine($"The house   quality {quality}, {living} sq ft, {age} years old, condition rated {condition}");
Console.WriteLine($"            sold for ${house.Price.Values[subject]:N0} — asked here at 10% more: ${asking:N0}");
Console.WriteLine();

// ── valuation: overpayment, in spreads ────────────────────────────────────────────────────────────

var everything = new Func<int, bool>(_ => true);
var valuation = Valuation.FairValueCoefficients(Design(everything, false), Prices(everything));
var spread = Valuation.SpreadOf(Design(everything, false), Prices(everything)).Values[0];
Matrix subjectRow = new double[,] { { quality, living, age, 1d } };
var fair = Valuation.FairValueOf(valuation, subjectRow).Values[0];
var excess = Valuation.ExcessInSpreads(asking, fair, spread);

// ── condition: repair exposure, priced within the house's own age band ───────────────────────────

var band = new Func<int, bool>(row => age < 30 ? house.HouseAge.Values[row] < 30 : house.HouseAge.Values[row] >= 30);
var perPoint = Condition.ConditionModel(Design(band, true), Prices(band)).Values[3];
var exposure = Condition.RepairExposure(condition, perPoint);

// ── credit: a hazard over a loan that does not exist ─────────────────────────────────────────────

var ltv = Credit.LoanToValueOf(asking, Seed);
var rate = Credit.RateOf(asking, Seed);
Matrix loan = new double[,] { { ltv, rate, 1d } };
Vector declared = new double[] { 4d, 20d, -6d };
var chance = Credit.DefaultChance(Credit.DefaultHazard((Tensor)loan, (Tensor)declared).Values[0]);
var loss = Credit.LossGivenDefault(asking, ltv);

// ── the agents: each believes its own number, scaled to an integer, and intends a verdict ────────

async Task<string> Intends<T>(IAsyncEnumerable<T> rows, Func<T, string> plan)
{
    await foreach (var row in rows)
    {
        return plan(row);
    }

    return "—";
}

var valuer = Val.ValuationagentEngineFactory.Create();
valuer.AssertAll([
    new Val.DesireFact("v", "Decided"), new Val.ExcessFact("v", (int)Math.Round(excess * 100)),
    new Val.ToleranceFact("v", 100), new Val.CeilingFact("v", 300),
]);

var inspector = Cond.ConditionagentEngineFactory.Create();
inspector.AssertAll([
    new Cond.DesireFact("c", "Decided"), new Cond.ExposureFact("c", (int)Math.Round(exposure / 1000)),
    new Cond.ToleranceFact("c", 10), new Cond.CeilingFact("c", 40),
]);

var underwriter = Cred.CreditagentEngineFactory.Create();
underwriter.AssertAll([
    new Cred.DesireFact("k", "Decided"), new Cred.ChanceFact("k", (int)Math.Round(chance * 10000)),
    new Cred.ToleranceFact("k", 1000), new Cred.CeilingFact("k", 2000),
]);

var valuationSays = await Intends(valuer.Intentions("v"), row => row.Plan);
var conditionSays = await Intends(inspector.Intentions("c"), row => row.Plan);
var creditSays = await Intends(underwriter.Intentions("k"), row => row.Plan);

Console.WriteLine($"  ValuationAgent   fair value ${fair:N0}, spread ${spread:N0}");
Console.WriteLine($"                   asking is {excess:+0.00;-0.00} spreads over                     intends {valuationSays}");
Console.WriteLine($"  ConditionAgent   rated {condition}, {Condition.PointsBelowTypical(condition)} points below typical × ${perPoint:N0} a point");
Console.WriteLine($"                   repair exposure ${exposure:N0}                            intends {conditionSays}");
Console.WriteLine($"  CreditAgent      loan-to-value {ltv:P1}, rate {rate:P2}, default chance {chance:P2}");
Console.WriteLine($"                   loss if it defaults ${loss:N0}                          intends {creditSays}");
Console.WriteLine("                   ⚠ THIS LOAN IS SYNTHESISED. houses.csv has no mortgages: the loan-to-value");
Console.WriteLine("                     and rate are invented from the price and a seed, and the hazard coefficients");
Console.WriteLine("                     are declared, not fitted. This line demonstrates a method and measures nothing.");
Console.WriteLine();

// ── the adjudicator: a veto, not a vote ──────────────────────────────────────────────────────────

var panel = Verdict.AdjudicatorEngineFactory.Create();

void Record(string agent, string plan)
{
    switch (plan)
    {
        case "Walk": panel.AssertAll([new Verdict.WalksFact(agent, "this")]); break;
        case "Negotiate": panel.AssertAll([new Verdict.HagglesFact(agent, "this")]); break;
        case "Buy": panel.AssertAll([new Verdict.BuysFact(agent, "this")]); break;
    }
}

Record("valuation", valuationSays);
Record("condition", conditionSays);
Record("credit", creditSays);

async Task<bool> Has(IAsyncEnumerable<string> rows)
{
    await foreach (var row in rows)
    {
        if (row == "this")
        {
            return true;
        }
    }

    return false;
}

var outcome = await Has(panel.WalkVerdicts()) ? "Walk"
    : await Has(panel.NegotiateVerdicts()) ? "Negotiate"
    : await Has(panel.BuyVerdicts()) ? "Buy" : "—";

Console.WriteLine($"  Adjudicator      {outcome}" + (outcome == "Walk" ? " — one agent seeing ruin outranks two seeing a bargain." : "."));
Console.WriteLine("                   The three disagree, and that is the design: each is right about its own question.");
Console.WriteLine();

// ── the reserve, had it gone ahead: sized against EVER being ruined ─────────────────────────────

Console.WriteLine("Had the purchase gone ahead anyway, what would it take to carry it? The claims against the buyer are");
Console.WriteLine("the repair exposure and the default loss together; the claim rate and the buyer's yearly set-aside are");
Console.WriteLine("ASSUMED for this scenario, not measured.");
Console.WriteLine();

const double ClaimRate = 0.5d;
var meanClaim = exposure + loss;

foreach (var setAside in new[] { 30000d, 20000d })
{
    var loading = Reserve.LoadingOf(setAside, ClaimRate, meanClaim);

    Console.Write($"  set aside ${setAside:N0} a year against claims of ${meanClaim:N0} arriving {ClaimRate} a year:  ");

    if (loading <= 0d)
    {
        // THE REFUSAL. Below the loading, ruin is certain and the formula would answer infinity. Neither a
        // large number nor infinity is the truth, so no number is printed.
        Console.WriteLine($"loading {loading:P1}");
        Console.WriteLine("    NO RESERVE IS REPORTED. The set-aside does not beat the expected claims, so ruin is certain");
        Console.WriteLine("    however much is held back — and any figure printed here would be a comfortable lie.");
    }
    else
    {
        var reserve = Reserve.ReserveFor(setAside, ClaimRate, meanClaim, ToleratedRuin);
        Console.WriteLine($"loading {loading:P1}");
        Console.WriteLine($"    reserve ${reserve:N0} keeps the chance of EVER running dry at or below {ToleratedRuin:P0}");
        Console.WriteLine($"    (Lundberg bound at that reserve: {Reserve.RuinBound(reserve, setAside, ClaimRate, meanClaim):P2}).");
        Console.WriteLine("    A simulation could only say ruin had not happened YET; the bound is the answer to 'ever'.");
    }

    Console.WriteLine();
}

// EACH THEOREM IS A METHOD, AND CALLING IT IS THE PROOF: the kernel replays it here, now, from the
// `.hp` embedded in this assembly -- nothing below is a verdict the build wrote down.
Console.WriteLine("And what the kernel proves as this runs:");

Func<Judged>[] theorems =
[
    AdjudicatorProofs.OneWalkIsEnough,
    AdjudicatorProofs.AHaggleIsAVote,
    AdjudicatorProofs.AHaggleContestsThePurchase,
    AdjudicatorProofs.ABuyIsAVote,
    RiskProofs.AnAssessedOfferWithinToleranceIsAcceptable,
    RiskProofs.AnAcceptableOfferIsNotAlsoRuinous,
];

foreach (var theorem in theorems)
{
    var verdict = theorem();
    Console.WriteLine($"  {verdict.Status,-8} {verdict.Name}");
}
