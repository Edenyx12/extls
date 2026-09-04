using extls.Core;
using extls.Core.Modules;
using NCalc;

namespace extls.Tools
{
    [ModuleName("calculator", "calc", "calculate")]
    public class Calculator : ModuleRaw
    {
        public Calculator()
        {
            name = "calculate";
            version = "1.1-stable";
            commands = null!;
        }

        public override void Help() =>
            Print.Line("for a mathematical calculation, ex. extls calc \"2 + 2 * (2 * 2)\"");

        public override bool DispatchRaw(string[] expression)
        {
            string indexpr = string.Empty;
            for (int i = 0; i < expression.Length; i++)
            {
                indexpr = expression.Length > 1 ? $"({i + 1})" : "";
                try
                {
                    var expr = new NCalc.Expression(expression[i], ExpressionOptions.DecimalAsDefault);

                    object result = expr.Evaluate()!;

                    Print.Line($"Result {indexpr}: {result}");
                }
                catch
                {
                    Print.Warning($"Result {indexpr}: Invalid mathematical expression.");
                }
            }

            return true;
        }
    }
}
