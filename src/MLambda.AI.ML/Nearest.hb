-- Nearest.hb — the models that do not train.
--
-- THE CONTRAST IS THE POINT. A Perceptron spends its fit compressing the data into weights and then
-- throws the data away. These keep every point and do the work at QUESTION time. Neither is better:
-- they trade memory for time, and a reader who has met both stops thinking "model" means "weights".
--
-- NAMED `Nearest`, NOT `Neighbours`, so it cannot be mistaken for `Prelude.Neighbours`, which is
-- where all the actual work is. Nothing below reimplements a distance.
--
-- THE CLASSES ARE ONE-HOT: `g` has one row per training point and one column per class, with a 1
-- where that point belongs. `z` is the points being asked about; `x` the points already known.
open Prelude.Neighbours

-- k-NEAREST NEIGHBOURS: the k closest known points vote, and the most votes wins.
fn classifyNear(z, x, g, k) ↦ knnClassify(z, x, g, k)

-- THE SAME VOTE AS A DISTRIBUTION -- what share of the k neighbours belong to each class.
fn sharesNear(z, x, g, k)   ↦ knnShares(z, x, g, k)

-- AND AS A NUMBER: the average of the k nearest known values.
fn regressNear(z, x, y, k)  ↦ knnRegress(z, x, y, k)

-- NEAREST CENTROID: average each class to one point, then ask which average is closest. It keeps
-- far less than k-NN -- one point per class instead of every point -- and that is the whole trade.
fn nearestCentre(z, x, g)   ↦ centroidPredict(z, x, g)
