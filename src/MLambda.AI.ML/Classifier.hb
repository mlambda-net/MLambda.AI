-- Classifier.hb — a class is one more index.
--
-- WHAT CHANGES FROM `Perceptron`: the last layer is `classes` wide instead of 1. That is ALL. Most
-- courses make classification look like a different subject from regression; it is the same network
-- with a wider last layer and targets written one column per class.
--
-- AND THE SOFTMAX LIVES WHERE THE CALLER ASKS FOR SHARES, not inside the network. `eval` answers
-- scores; `shares` answers a distribution. Baking the softmax in would make every caller pay for it
-- and make the scores unreachable -- which matters, because the scores are what you train on, and
-- because the softmax is monotone: it never changes which class wins, only how sure it sounds.
open Prelude.Networks
open Prelude.Kernels
open Prelude.LinearAlgebra

model Classifier(inputs, hidden, classes, seed) : Learner {

  weight first  : Tensor[inputs, hidden]  from gaussian(seed) · 0.5
  weight second : Tensor[hidden, classes] from gaussian(seed) · 0.5

  def train(x, y, rate) {
    first  ≔ stepFirst(rate, x, y, first, second)
    second ≔ stepSecond(rate, x, y, first, second)
  }

  def eval(x)   ≔ outputs(x, first, second)
  def shares(x) ≔ softmaxRows(outputs(x, first, second))

  -- WHICH CLASS WON, one-hot per row. Read from the scores, not the shares -- and the test asserts
  -- the two agree, because a monotone map cannot reorder them.
  def winner(x) ≔ lowest(0 − outputs(x, first, second))

  def loss(x, y) ≔ einsum("ij->", (outputs(x, first, second) − y) · (outputs(x, first, second) − y)) / rows(x)
}
