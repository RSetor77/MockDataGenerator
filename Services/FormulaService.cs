using Jint;
using System;

namespace MockDataGenerator.Services
{
    public static class FormulaService
    {
        public static readonly Engine engine = new(options =>
        {
            options.LimitRecursion(20);
            options.TimeoutInterval(TimeSpan.FromSeconds(5));
        });

        static FormulaService()
        {
            engine.SetValue("uuid", new Func<string>(() => Guid.NewGuid().ToString()));
            engine.SetValue("random", new Func<int, int, int>((min, max) => Random.Shared.Next(min, max)));
            engine.SetValue("now", new Func<string>(() => DateTime.Now.ToString()));
        }
    }
}
