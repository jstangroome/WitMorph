using System;
using System.Diagnostics;

namespace WitMorph.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new ConsoleTraceListener());

            var collectionUri = new Uri(args[0]);
            var projectName = args[1];
            var newProcessTemplateName = args[2];

            var engine = new MorphEngine();

            var actions = engine.GenerateActions(collectionUri, projectName, newProcessTemplateName);
            engine.Apply(collectionUri, projectName, actions);

            System.Console.WriteLine("DONE");
        }
    }
}
