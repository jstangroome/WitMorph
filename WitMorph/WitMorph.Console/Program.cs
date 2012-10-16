using System;
using System.Diagnostics;
using WitMorph.Actions;

namespace WitMorph.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new ConsoleTraceListener());

            var collectionUri = args[0];
            var projectName = args[1];

            var c = new Compare();

            var actions = c.Do(new Uri(collectionUri), projectName);
            var context = new ExecutionContext(new Uri(collectionUri), projectName);
            foreach (var action in actions)
            {
                action.Execute(context);
            }

            System.Console.WriteLine("DONE");
        }
    }
}
