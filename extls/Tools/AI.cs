using extls.Core;
using System.Text.Json;

namespace extls.Tools
{
    public class AI : Module
    {
        public AI()
        {
            name = "ai";
            version = "0.1a";
            commands = new[] {
                new HelpSlot("calc-m, calc-memory", "calculates memory consumption based on model weights and quantization in bits",
                     new [] {"\"model weights\" - ex. 23b, 1t, 2m, 3k etc.",
                             "\"model quantization\" - ex 8, 16, 32, etc." },
                             "extls ai calc-m 8b 16 (result 14.90 GB)"),
            };
        }

        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            switch (args[0])
            {
                case "calc-m": case "calc-memory":
                    Calculate(Utils.RemoveZeroCommand(args));
                    break;

                default:
                    Utils.InvalidOperation();
                    break;
            }

            return true;
        }

        private static void Calculate(string[] args)
        {
            if (args.Length == 0)
            {
                Utils.InvalidOperation();
                return;
            }

            double parameters;
            int quant;

            try
            {
                switch (args[0][args[0].Length-1].ToString().ToLower())
                {
                    case "k": Parameters(1000f); break;
                    case "m": Parameters(1000000f); break;
                    case "b": Parameters(1000000000f); break;
                    case "t": Parameters(1000000000000f); break;

                    default: parameters = double.Parse(args[0]);
                        break;
                }                

                quant = int.Parse(args[1]);

                void Parameters(float f)
                {
                    string num = "";
                    for (int i = 0; i < args[0].Length - 1; i++)
                        num += args[0][i];
                    parameters = float.Parse(num) * f;
                }
            }
            catch { Utils.InvalidOperation(); return; }

            if (parameters == 0 || quant == 0)
            {
                Utils.InvalidOperation();
                return;
            }

            double usegb = 
                ((quant / 8d)         // bytes
                * parameters)         // bytes per all parameters
                / 1024 / 1024 / 1024; // / kb / mb / gb

            Print.Line($"Needed memory: {usegb:F2} GB");
            Print.Debug($"parameters: {parameters}");
            Print.Debug($"Q: {quant}");
        }
    }
}
