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
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "algebra": Algebra(); break;
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
}
