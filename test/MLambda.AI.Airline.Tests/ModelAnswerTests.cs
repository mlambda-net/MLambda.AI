// ModelAnswerTests.cs — reading a model's JSON without reading generously.
namespace MLambda.AI.Airline.Tests;

public class ModelAnswerTests
{
    [Fact]
    public void A_well_formed_draft_is_read_as_written()
    {
        var draft = Draft.Parse(FakeLlm.Drafted("bereavement_fare", "yes", "nothing", "Sorry for your loss."))!;

        Assert.Equal(("bereavement_fare", true, "nothing", "Sorry for your loss.", true), (draft.Request, draft.Bereavement, draft.Promise, draft.Text, draft.FromModel));
        Assert.Empty(draft.Before);
        Assert.True(draft.InScope);
    }

    [Fact]
    public void A_draft_in_a_code_fence_with_prose_around_it_is_still_read()
    {
        var draft = Draft.Parse("Here you go:\n```json\n{\"request\": \"REFUND\", \"promise\": \"Refund\"}\n```");

        Assert.Equal("refund", draft!.Request);
        Assert.Equal("refund", draft.Promise);
    }

    [Fact]
    public void An_unknown_request_is_other_but_an_unknown_promise_is_kept_so_nothing_backs_it()
    {
        var draft = Draft.Parse(FakeLlm.Drafted("upgrade", "no", "free_upgrade", "Enjoy business class."))!;

        Assert.Equal(Draft.Other, draft.Request);
        Assert.False(draft.InScope);
        Assert.Equal("free_upgrade", draft.Promise);
    }

    [Fact]
    public void A_missing_promise_is_unstated_not_nothing()
    {
        Assert.Equal("unstated", Phrasing.Parse("""{"reply": "Hello"}""")!.Promise);
        Assert.Equal("unstated", Draft.Parse("""{"request": "refund"}""")!.Promise);
    }

    [Fact]
    public void No_object_or_no_reply_reads_as_nothing_to_use()
    {
        Assert.Null(Draft.Parse("I cannot help with that."));
        Assert.Null(Draft.Parse("{not json}"));
        Assert.Null(Phrasing.Parse("""{"promise": "nothing"}"""));
    }

    [Fact]
    public void Without_a_model_the_request_is_guessed_from_words_and_nothing_is_promised()
    {
        var guessed = Draft.Guess("My father died, is there a discount?");

        Assert.Equal(("bereavement_fare", true, "nothing", "", false), (guessed.Request, guessed.Bereavement, guessed.Promise, guessed.Text, guessed.FromModel));
        Assert.Empty(guessed.Before);
        Assert.Equal("refund", Draft.Guess("I want my money back").Request);
        Assert.Equal(Draft.Other, Draft.Guess("Where is my suitcase?").Request);
    }

    [Fact]
    public void The_steps_before_asking_are_read_in_order_and_only_steps_the_book_knows_are_kept()
    {
        Assert.Equal(["fly"], Draft.Parse(FakeLlm.Drafted("refund", "no", "nothing", "", "fly"))!.Before);
        Assert.Equal(["fly"], Draft.Parse(FakeLlm.Drafted("refund", "no", "nothing", "", "pack", "FLY", "dance"))!.Before);
        Assert.Empty(Draft.Parse("""{"request": "refund", "before": "fly"}""")!.Before);
    }

    [Fact]
    public void Without_a_model_flying_then_asking_is_still_read_as_a_plan()
    {
        Assert.Equal(["fly"], Draft.Guess("so I can fly on RFD512 and ask for a refund?").Before);
        Assert.Empty(Draft.Guess("I want a refund for RFD512").Before);
    }
}
