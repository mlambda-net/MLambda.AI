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
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "algebra": Algebra(); break;
    case "calculus": Calculus(); break;
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
