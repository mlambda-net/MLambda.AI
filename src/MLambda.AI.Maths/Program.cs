// Program.cs — a switch, not a framework.
//
// IF A LINE HERE LOOKS LIKE MATHEMATICS, IT IS IN THE WRONG FILE. The mathematics is in the `.hb`
// files and in the laws they open; this file hands a number to a sample and prints what came back.
//
// THERE IS NO REGISTRY AND NO LESSON INTERFACE. A new sample is a case and a method.
using MLambda.AI.Maths;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.Maths — pass a sample name:");
    Console.WriteLine();
    Console.WriteLine("  Algebra     the laws are the knowledge, and they say which one acted");
    Console.WriteLine("  Calculus    a derivative taken before you ran anything — and one that was not");
    Console.WriteLine("  Solve       what the CAS refuses to guess, and the two laws that end the refusal");
    Console.WriteLine("  Linear      one contraction underneath, and two words spelled solve");
    Console.WriteLine("  Proof       three grades of certainty, and the rule the CAS spent");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "algebra": Algebra(); break;
    case "calculus": Calculus(); break;
    case "solve": Solve(); break;
    case "linear": Linear(); break;
    case "proof": Proof(); break;
    default:
        Console.Error.WriteLine($"There is no sample called '{args[0]}'. Run with no arguments for the list.");
        return 1;
}

return 0;

static void Algebra()
{
    Console.WriteLine("sin(x)² + cos(x)² — and what the build did with it.");
    Console.WriteLine();
    Console.WriteLine($"  energy(0.009)            {MLambda.AI.Maths.Algebra.Energy(0.009):R}");
    Console.WriteLine($"  the same sum, computed   {(Math.Sin(0.009) * Math.Sin(0.009)) + (Math.Cos(0.009) * Math.Cos(0.009)):R}");
    Console.WriteLine();
    Console.WriteLine("Those are different numbers. `pythagorean` removed the sine and the cosine while");
    Console.WriteLine("this project was building, so the first one is a literal the method returns —");
    Console.WriteLine("not a rounding of the second.");
    Console.WriteLine();

    var simplified = MLambda.AI.Maths.Algebra.Compute.Simplify("x · 1 + 0");
    var narrow = OneLaw.Compute.Simplify("x · 1 + 0");

    Console.WriteLine("And the same laws, called at run time instead:");
    Console.WriteLine();
    Console.WriteLine($"  Algebra.hb says    x · 1 + 0  ↦  {simplified}   by [{string.Join(", ", simplified.Steps)}]");
    Console.WriteLine($"  OneLaw.hb says     x · 1 + 0  ↦  {narrow}   by [{string.Join(", ", narrow.Steps)}]");
    Console.WriteLine();
    Console.WriteLine("Two files in one assembly, two different answers. OneLaw.hb opens nothing and");
    Console.WriteLine("declares one law, so its CAS can remove the `· 1` and not the `+ 0`. The answer");
    Console.WriteLine("arrived with the reason, which is the only reason to trust it.");
}

static void Calculus()
{
    Console.WriteLine("∂ x² / ∂ x, taken while this project was building.");
    Console.WriteLine();
    Console.WriteLine($"  slope(3)    {MLambda.AI.Maths.Calculus.Slope(3)}");
    Console.WriteLine($"  chain(3)    {MLambda.AI.Maths.Calculus.Chain(3)}");
    Console.WriteLine();
    Console.WriteLine("The generated C# for `slope` is `(2d * x)`. There is no derivative in it, and no");
    Console.WriteLine("differentiation happened just now.");
    Console.WriteLine();

    var derivative = MLambda.AI.Maths.Calculus.Compute.Derivate("sin(x²)", "x");
    var refused = MLambda.AI.Maths.Calculus.Compute.Derivate("tan(x)", "x");

    Console.WriteLine("The same laws, at run time:");
    Console.WriteLine();
    Console.WriteLine($"  ∂ sin(x²) / ∂ x  ↦  {derivative}");
    Console.WriteLine($"                       by [{string.Join(", ", derivative.Steps)}]");
    Console.WriteLine();
    Console.WriteLine($"  ∂ tan(x) / ∂ x   ↦  {refused}");
    Console.WriteLine("                       no law names tan, so the ∂ is still there.");
    Console.WriteLine();
    Console.WriteLine("Calculus.hb declares `tangent` for exactly that derivative. Look for it in the");
    Console.WriteLine("generated class and it is not there — a doorway that still holds a derivative is");
    Console.WriteLine("not emitted, and the build says nothing at all. The CAS above is the kinder of");
    Console.WriteLine("the two: it hands back what it could not finish.");
}

static void Solve()
{
    Console.WriteLine("2 · x + 3 = 7 — asked of two files.");
    Console.WriteLine();
    Console.WriteLine($"  Algebra.hb   {MLambda.AI.Maths.Algebra.Compute.Solve("2 · x + 3 = 7", "x")}");
    Console.WriteLine($"  Solve.hb     {MLambda.AI.Maths.Solve.Compute.Solve("2 · x + 3 = 7", "x")}");
    Console.WriteLine();
    Console.WriteLine("Algebra.hb has fifteen laws and cannot do it. Every one of them is a term law,");
    Console.WriteLine("which moves a sub-term; isolating x means moving the equation. Solve.hb declares");
    Console.WriteLine("two laws with ⇔ and that is the whole difference:");
    Console.WriteLine();
    Console.WriteLine("    law sub_add = a + b = c ⇔ a = c − b");
    Console.WriteLine("    law div_mul = a · b = c ⇔ b = c / a");
    Console.WriteLine();
    Console.WriteLine("No prelude ships them. Transposition needs what you divide by to be non-zero, and");
    Console.WriteLine("a rewrite rule cannot check that — so the assumption is yours to declare, in the");
    Console.WriteLine("file that leans on it. The refusal was the honest answer.");
}

static void Linear()
{
    MLambda.Hilbert.Runtime.Matrix a = new double[,] { { 1, 2 }, { 3, 4 } };

    var named = MLambda.AI.Maths.Linear.Product(a, a);
    var spelled = MLambda.AI.Maths.Linear.Contracted(a, a);

    Console.WriteLine("[1 2; 3 4] squared, by two names for one contraction.");
    Console.WriteLine();
    Console.WriteLine($"  mul(a, a)                      [{named[0, 0]} {named[0, 1]}; {named[1, 0]} {named[1, 1]}]");
    Console.WriteLine($"  einsum(\"ij,jk->ik\", a, a)      [{spelled[0, 0]} {spelled[0, 1]}; {spelled[1, 0]} {spelled[1, 1]}]");
    Console.WriteLine();
    Console.WriteLine("`mul` IS that einsum — the prelude defines it that way and a `def` is inlined, so");
    Console.WriteLine("the two doorways compile to the same arithmetic. Transpose, dot and outer are the");
    Console.WriteLine("same contraction with different indices.");
    Console.WriteLine();

    MLambda.Hilbert.Runtime.Matrix diagonal = new double[,] { { 2, 0 }, { 0, 3 } };
    MLambda.Hilbert.Runtime.Vector rhs = new double[] { 4, 9 };
    var x = MLambda.AI.Maths.Linear.Solved(diagonal, rhs);

    Console.WriteLine("And two different things are called solve:");
    Console.WriteLine();
    Console.WriteLine($"  solve(a, b)   here, numeric      x = [{x[0]:0.######} {x[1]:0.######}]   conjugate gradient");
    Console.WriteLine($"  Compute.Solve  in Solve.hb       {MLambda.AI.Maths.Solve.Compute.Solve("2 · x + 3 = 7", "x")}          rewriting, by ⇔ laws");
    Console.WriteLine();
    Console.WriteLine("One takes numbers and returns numbers. The other takes an equation and returns an");
    Console.WriteLine("expression. They share a word and nothing else.");
}

static void Proof()
{
    var found = MLambda.AI.Maths.Algebra.Compute.Demonstrate("theorem t : x · 1 + 0 = x");
    var searched = found as MLambda.Hilbert.Algebra.Demonstrated;
    var verified = searched is null ? null : MLambda.AI.Maths.Algebra.Compute.Proof(searched.Theorem);

    Console.WriteLine("Three grades of certainty, weakest first.");
    Console.WriteLine();
    Console.WriteLine($"  Demonstrate   searched, and found    {found}");
    Console.WriteLine($"  Proof         checked those steps    {verified}");
    Console.WriteLine($"  the .hp       checked by the build   {MathsProofs.DerivOfXSquared().Status}");
    Console.WriteLine();
    Console.WriteLine("The first may fail to find anything — the search is bounded, on purpose, so it");
    Console.WriteLine("always comes back. The second only checks what you hand it. The third ran before");
    Console.WriteLine("this program was allowed to exist: `deriv_of_x_squared` is the L2 derivative of");
    Console.WriteLine("x², proved from the product rule, and a break in it is a build failure.");
    Console.WriteLine();

    var gradient = MLambda.AI.Maths.Calculus.Compute.Derivate("(w · x − y)²", "w");

    Console.WriteLine("And the reason a mathematics section belongs beside the AI ones:");
    Console.WriteLine();
    Console.WriteLine("  Perceptron.hb's loss, for one linear unit    (w · x − y)²");
    Console.WriteLine($"  ∂ of it in w, by the same thirteen laws      {gradient}");
    Console.WriteLine($"                                              by [{string.Join(", ", gradient.Steps)}]");
    Console.WriteLine();
    Console.WriteLine("That is the update the ML subject writes by hand. Nobody derived it there; it was");
    Console.WriteLine("stated. Here it is derived, and a test fails if the two ever stop agreeing.");
}
