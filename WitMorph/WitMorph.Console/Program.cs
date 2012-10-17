using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WitMorph.Actions;

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
            foreach (var action in actions)
            {
                System.Console.WriteLine(action.ToString());
            }

            var filteredActions = actions.Where(a => !(a is DestroyWitdMorphAction)); // optionally skip deleting extra work item items

            engine.Apply(collectionUri, projectName, actions, Path.GetTempPath()); //TODO replace temp path with something useful

            System.Console.WriteLine("DONE");
        }
    }
}
