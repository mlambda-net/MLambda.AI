// Program.cs — fits each model on a small fixture and prints the error falling.
//
// IF A LINE HERE LOOKS LIKE MATHEMATICS, IT IS IN THE WRONG FILE. Every formula -- the fit, the
// network, the loss, the vote -- is in a `.hb` file. This one builds fixtures, calls the generated
// code, and prints. The loss printed below is the model's own `loss`, not a copy computed here.
//
// AND THE FIXTURES ARE TINY ON PURPOSE. A loss that falls on four points proves the training loop
// works. It says nothing about generalisation, and nothing printed here pretends otherwise.
using MLambda.AI.ML;
using MLambda.Hilbert.Runtime;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.ML — pass a sample name:");
    Console.WriteLine();
    Console.WriteLine("  Line        fit a straight line, and recover one you chose");
    Console.WriteLine("  Perceptron  a model with weights, learning what a line cannot");
    Console.WriteLine("  Classifier  a class is one more index — and what a missing bias costs");
    Console.WriteLine("  Nearest     the models that do not train");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "line": Line(); break;
    case "perceptron": Perceptron(); break;
    case "classifier": Classifier(); break;
    case "nearest": Nearest(); break;
    default:
        Console.Error.WriteLine($"There is no sample called '{args[0]}'. Run with no arguments for the list.");
        return 1;
}

return 0;

static string Row(IReadOnlyList<double> values, int width) =>
    string.Join("  ", values.Select(v => v.ToString("F3").PadLeft(width)));

static void Line()
{
    Console.WriteLine("Four points from y = 3x + 1, with no noise in them.");
    Console.WriteLine("The design matrix carries a column of ones: that is how a line gets an intercept.");
    Console.WriteLine();

    Matrix inputs = new double[,] { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
    Vector outputs = new double[] { 4, 7, 10, 13 };

    var line = MLambda.AI.ML.Line.LineOf(inputs, outputs);

    Console.WriteLine($"  recovered   y = {line.Values[0]:F3}x + {line.Values[1]:F3}");

    Matrix further = new double[,] { { 10, 1 } };
    var predicted = MLambda.AI.ML.Line.PredictWith((Tensor)line, (Tensor)further);

    Console.WriteLine($"  at x = 10   predicts {predicted.Values[0]:F3}   (the line says 31)");
    Console.WriteLine($"  spread      {MLambda.AI.ML.Line.SpreadOf(inputs, outputs).Values[0]:F3}   no noise in, no spread out");

    Vector noisy = new double[] { 4, 8, 9, 13 };

    Console.WriteLine();
    Console.WriteLine("Now the same x with some noise in y:");
    Console.WriteLine($"  spread      {MLambda.AI.ML.Line.SpreadOf(inputs, noisy).Values[0]:F3}");
    Console.WriteLine();
    Console.WriteLine("A fit without its spread invites false confidence. Both come from one .hb file.");
}

static void Perceptron()
{
    Console.WriteLine("XOR: the smallest problem one layer cannot solve and two layers can.");
    Console.WriteLine();

    Matrix xs = new double[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
    Matrix ys = new double[,] { { 0 }, { 1 }, { 1 }, { 0 } };

    var net = new MLambda.AI.ML.Perceptron(2, 8, seed: 7);

    Console.WriteLine("  step   loss");
    Console.WriteLine("  ────   ─────────");

    for (var step = 0; step <= 500; step++)
    {
        if (step is 0 or 10 or 50 or 100 or 250 or 500)
        {
            Console.WriteLine($"  {step,4}   {net.Loss(xs, ys).Values[0]:G4}");
        }

        net.Train(xs, ys, 0.5);
    }

    Console.WriteLine();
    Console.WriteLine($"  answers  {Row(net.Eval(xs).Values, 6)}   (XOR is 0 1 1 0)");
    Console.WriteLine();
    Console.WriteLine("One answer of the four was never learned. This network has no bias, so at");
    Console.WriteLine("(0, 0) it answers zero whatever its weights — and XOR's target there happens");
    Console.WriteLine("to be zero. Run `Classifier` to see the same architecture get it wrong.");
}

static void Classifier()
{
    Console.WriteLine("XOR again, written as two classes: column 0 is 'off', column 1 is 'on'.");
    Console.WriteLine("The network is the Perceptron with a wider last layer. That is all a class is.");
    Console.WriteLine();

    Matrix ys = new double[,] { { 1, 0 }, { 0, 1 }, { 0, 1 }, { 1, 0 } };

    Matrix bare = new double[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
    var noBias = new MLambda.AI.ML.Classifier(2, 8, 2, seed: 1);

    for (var step = 0; step < 500; step++)
    {
        noBias.Train(bare, ys, 0.5);
    }

    Console.WriteLine("Without a bias column, after 500 steps:");
    Console.WriteLine($"  scores at (0,0)   {Row(noBias.Eval(bare).Values.Take(2).ToArray(), 6)}   (should be 1 0)");
    Console.WriteLine("  Zero for both. Every unit multiplies (0, 0) by its weights, and no amount");
    Console.WriteLine("  of training moves a product of zero.");
    Console.WriteLine();

    Matrix biased = new double[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 } };

    foreach (var seed in new[] { 1d, 7d })
    {
        var net = new MLambda.AI.ML.Classifier(3, 8, 2, seed: seed);

        for (var step = 0; step < 500; step++)
        {
            net.Train(biased, ys, 0.5);
        }

        var right = net.Winner(biased).Values.SequenceEqual(ys.Values);

        Console.WriteLine($"With a column of ones, seed {seed}:   loss {net.Loss(biased, ys).Values[0]:G4}   every row right: {right}");
    }

    Console.WriteLine();
    Console.WriteLine("By step 500, seed 7 has five of its eight hidden units dead. A dead ReLU unit");
    Console.WriteLine("passes back a zero gradient and never recovers, and the three survivors cannot");
    Console.WriteLine("tell row 0 from row 2 — so the weights stop changing entirely. That is gradient descent");
    Console.WriteLine("working correctly, and it is why real training restarts from several seeds.");
}

static void Nearest()
{
    Console.WriteLine("Class A near the origin, class B near (10, 10). Nothing is trained: every");
    Console.WriteLine("answer below is worked out at the moment it is asked.");
    Console.WriteLine();

    Tensor known = (Matrix)new double[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 10, 10 }, { 10, 11 }, { 11, 10 } };
    Tensor classes = (Matrix)new double[,] { { 1, 0 }, { 1, 0 }, { 1, 0 }, { 0, 1 }, { 0, 1 }, { 0, 1 } };
    Tensor asked = (Matrix)new double[,] { { 0.5, 0.5 }, { 10.5, 10.5 } };

    var knn = MLambda.AI.ML.Nearest.ClassifyNear(asked, known, classes, 2d).Values;
    var centre = MLambda.AI.ML.Nearest.NearestCentre(asked, known, classes).Values;

    Console.WriteLine("  asked           k-NN (k=2)   nearest centroid");
    Console.WriteLine($"  (0.5, 0.5)      {(knn[0] == 1 ? "A" : "B"),-12} {(centre[0] == 1 ? "A" : "B")}");
    Console.WriteLine($"  (10.5, 10.5)    {(knn[3] == 1 ? "B" : "A"),-12} {(centre[3] == 1 ? "B" : "A")}");
    Console.WriteLine();
    Console.WriteLine("They agree. k-NN kept all six points to do it; nearest-centroid kept two");
    Console.WriteLine("averages. On clusters this clean the cheaper one is the right one — and a");
    Console.WriteLine("reader who has seen both stops thinking 'model' means 'weights'.");
}
