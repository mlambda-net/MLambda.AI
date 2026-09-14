// ModelAnswerTests.cs — reading a model's JSON without reading generously.
namespace MLambda.AI.Airline.Tests;

public class ModelAnswerTests
{
    [Fact]
    public void A_well_formed_draft_is_read_as_written()
    {
        var draft = Draft.Parse(FakeLlm.Drafted("bereavement_fare", "yes", "nothing", "Sorry for your loss."))!;

        Assert.Equal(new Draft("bereavement_fare", true, "nothing", "Sorry for your loss.", FromModel: true), draft);
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
        Assert.Equal(new Draft("bereavement_fare", true, "nothing", "", FromModel: false), Draft.Guess("My father died, is there a discount?"));
        Assert.Equal("refund", Draft.Guess("I want my money back").Request);
        Assert.Equal(Draft.Other, Draft.Guess("Where is my suitcase?").Request);
    }
}
