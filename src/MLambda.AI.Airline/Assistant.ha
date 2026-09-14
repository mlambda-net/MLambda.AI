-- Assistant.ha — decides what kind of reply a message gets, before anyone writes a word of it.
--
-- THREE OUTCOMES, AND THE MODEL CHOOSES NONE OF THEM. The host believes two things about a message —
-- whether it is about something the policy book covers, and whether a booking was found for it — and the
-- agent intends from those. A booking reference is read from the message by the host, against the
-- booking records; the model's reading of the message is never consulted here.
--
-- AN HONEST FAILURE IS A REPLY. A question the book does not cover is not answered from the model's
-- general knowledge: it is `impossible`, and the reason below is what the customer is told.
agent Assistant(id: String) : Policy
{
  belief topic(scope: String)
  belief booking(found: String)
  belief reviewed(verdict: String)
  belief replied(done: Int)

  desire Answered reachable by [Answer, Clarify]

  commit single_minded
  attention Listening initially

  intend Answer  for Answered when B(self) topic("in") ∧ B(self) booking("yes")
  intend Clarify for Answered when B(self) topic("in") ∧ B(self) booking("no")

  achieved Answered when B(self) replied(1)
  impossible Answered when B(self) topic("out") because "That is not about a refund or a bereavement fare, and no written policy here covers it — please contact an agent"

  -- LABELS, NOT PROVED ROOTS. `Reply.hk` names its plans `AnswerSteps` and `ClarifySteps`: they wait on
  -- the host over channels, and a plan an agent starts directly may take none (HK0051).
  plan Answer  ↦ Reply.Answer
  plan Clarify ↦ Reply.Clarify
}
