// PolicyTests.cs — the norms Policy.hs derives: what the airline and the assistant are obliged, forbidden and permitted to do.
namespace MLambda.AI.Airline.Tests;

public class PolicyTests
{
    private static readonly Rulebook Rules = new(Shipped.Book);

    private static readonly string[] References = ["MOF240", "FUT315", "RFD512", "CXL777"];

    private static Task<Decision> Decide(string reference, string request, bool bereavement = false) =>
        Rules.DecideAsync("w1", Shipped.Booking(reference), request, bereavement);

    private static IEnumerable<string> Written(IEnumerable<Norm> norms) =>
        norms.Select(n => $"{n.Written} {n.Policy.Id}");

    [Fact]
    public async Task The_Moffatt_case_a_bereavement_fare_after_the_trip_is_forbidden_under_BRV_2()
    {
        var decision = await Decide("MOF240", "bereavement_fare", bereavement: true);

        Assert.Equal(["F(airline) bereavement_fare BRV-2"], Written(decision.Forbidden));
        Assert.Empty(decision.Obliged);
    }

    [Fact]
    public async Task The_same_bereavement_asked_for_before_the_trip_is_obligatory_under_BRV_1()
    {
        var decision = await Decide("FUT315", "bereavement_fare", bereavement: true);

        Assert.Equal(["O(airline) bereavement_fare BRV-1"], Written(decision.Obliged));
        Assert.Empty(decision.Forbidden);
    }

    [Fact]
    public async Task A_bereavement_fare_without_a_bereavement_is_forbidden_under_BRV_1()
    {
        var decision = await Decide("FUT315", "bereavement_fare", bereavement: false);

        Assert.Equal(["F(airline) bereavement_fare BRV-1"], Written(decision.Forbidden));
        Assert.Empty(decision.Obliged);
    }

    [Fact]
    public async Task A_refundable_fare_not_yet_flown_obliges_a_refund_under_REF_1()
    {
        var decision = await Decide("RFD512", "refund");

        Assert.Equal(["O(airline) refund REF-1"], Written(decision.Obliged));
        Assert.Empty(decision.Forbidden);
    }

    [Fact]
    public async Task A_non_refundable_fare_forbids_a_refund_and_obliges_a_credit_under_REF_2()
    {
        var decision = await Decide("FUT315", "refund");

        Assert.Equal(["F(airline) refund REF-2"], Written(decision.Forbidden));
        Assert.Equal(["O(airline) travel_credit REF-2"], Written(decision.Obliged));
    }

    [Fact]
    public async Task A_flight_the_airline_cancelled_obliges_a_refund_under_REF_3_whatever_the_fare()
    {
        var decision = await Decide("CXL777", "refund");

        Assert.Equal(["O(airline) refund REF-3"], Written(decision.Obliged));
        Assert.Empty(decision.Forbidden);
    }

    [Fact]
    public async Task A_trip_already_flown_forbids_a_refund_under_REF_4()
    {
        var decision = await Decide("MOF240", "refund");

        Assert.Equal(["F(airline) refund REF-4"], Written(decision.Forbidden));
        Assert.Empty(decision.Obliged);
    }

    [Fact]
    public async Task What_is_not_forbidden_is_permitted()
    {
        var decision = await Decide("FUT315", "refund");

        Assert.Equal(["bereavement_fare", "travel_credit"], await Rules.PermittedAsync(decision, Rulebook.Airline));
    }

    [Fact]
    public async Task The_assistant_is_permitted_to_promise_only_what_the_airline_is_obliged_to_give()
    {
        var decision = await Decide("FUT315", "refund");

        Assert.Equal(["travel_credit"], await Rules.PermittedAsync(decision, Rulebook.Assistant));
    }

    [Fact]
    public async Task Promising_what_the_airline_is_forbidden_to_give_violates_CHAT_1()
    {
        var decision = await Decide("MOF240", "bereavement_fare", bereavement: true);

        var broken = await Rules.ViolatedByAsync(decision, "bereavement_fare");

        Assert.Equal("F(assistant) bereavement_fare", broken!.Written);
        Assert.Equal("CHAT-1", broken.Policy.Id);
        Assert.NotNull(await Rules.ViolatedByAsync(decision, "refund"));
    }

    [Fact]
    public async Task Promising_what_the_airline_is_obliged_to_give_violates_nothing_and_neither_does_promising_nothing()
    {
        var decision = await Decide("FUT315", "refund");

        Assert.Null(await Rules.ViolatedByAsync(decision, "travel_credit"));
        Assert.Null(await Rules.ViolatedByAsync(decision, Rulebook.Nothing));
        Assert.NotNull(await Rules.ViolatedByAsync(decision, "refund"));
    }

    [Fact]
    public async Task A_remedy_the_model_invented_is_owed_by_nobody_so_promising_it_is_a_violation()
    {
        var decision = await Decide("CXL777", "refund");

        Assert.Null(await Rules.ViolatedByAsync(decision, "refund"));
        Assert.Equal("CHAT-1", (await Rules.ViolatedByAsync(decision, "full_refund_and_voucher"))!.Policy.Id);
    }

    [Fact]
    public async Task At_the_airline_s_ideal_situation_what_it_is_obliged_to_do_is_done()
    {
        var decision = await Decide("FUT315", "refund");

        Assert.Equal(["travel_credit"], await Rules.IdeallyAsync(decision, Rulebook.Airline));
    }

    [Fact]
    public async Task And_where_nothing_is_obligatory_the_ideal_situation_does_nothing()
    {
        var decision = await Decide("MOF240", "bereavement_fare", bereavement: true);

        Assert.Empty(await Rules.IdeallyAsync(decision, Rulebook.Airline));
    }

    [Fact]
    public async Task The_book_never_obliges_and_forbids_the_same_act_on_any_booking()
    {
        foreach (var reference in References)
        {
            foreach (var request in Rulebook.Remedies)
            {
                foreach (var bereavement in new[] { true, false })
                {
                    Assert.Empty(await Rules.ConflictsAsync("w1", Shipped.Booking(reference), request, bereavement));
                }
            }
        }
    }

    [Fact]
    public async Task Every_refund_or_bereavement_request_on_every_booking_is_decided_by_a_policy_with_wording()
    {
        foreach (var reference in References)
        {
            foreach (var request in new[] { "refund", "bereavement_fare" })
            {
                var decision = await Decide(reference, request, bereavement: true);

                Assert.False(decision.Uncovered, $"{request} on {reference} is decided by no policy");
                Assert.All(decision.Obliged.Concat(decision.Forbidden), n => Assert.NotEmpty(n.Policy.Text));
            }
        }
    }
}
