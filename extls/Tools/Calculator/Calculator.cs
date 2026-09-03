using extls.Core;
using NCalc;

namespace extls.Tools
{
    [ModuleName("calculator", "calc", "calculate")]
    public class Calculator : Module
    {
        public Calculator()
        {
            name = "calculate";
            version = "1.0-stable";
            commands = null!;
        }

        public override void Help() =>
            Print.Line("for a mathematical calculation, ex. extls calc \"2 + 2 * (2 * 2)\"");
        
        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;
            Calc(args[0]);
            return true;
        }

        private static void Calc(string expression)
        {
            try
            {
                var expr = new NCalc.Expression(expression, ExpressionOptions.DecimalAsDefault);

                object result = expr.Evaluate()!;

                Print.Line($"Result: {result}");
            }
            catch
            {
                Print.Error("Invalid mathematical expression.");
            }
        }

    }
}
