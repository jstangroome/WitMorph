using System;
using System.Diagnostics;
using System.IO;

namespace WitMorph.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                System.Console.WriteLine("usage: {0} <collection uri> <project name to morph> <project name to use as template>",
                    Process.GetCurrentProcess().ProcessName);
                return;
            }

            var collectionUri = new Uri(args[0]);
            var projectName = args[1];
            var templateProjectName = args[2];

            var factory = new ProcessTemplateFactory();

            var currentTemplate = factory.FromActiveTeamProject(collectionUri, projectName);
            var goalTemplate = factory.FromActiveTeamProject(collectionUri, templateProjectName);

            var diffEngine = new DiffEngine(ProcessTemplateMap.ConvertScrum2ToAgile6());
            var differences = diffEngine.CompareProcessTemplates(currentTemplate, goalTemplate);

            var engine = new MorphEngine();

            var actions = engine.GenerateActions(differences);
            foreach (var action in actions)
            {
                System.Console.WriteLine(action.ToString());
            }

            //var filteredActions = actions.Where(a => !(a is DestroyWitdMorphAction)); // optionally skip deleting extra work item items

            engine.Apply(collectionUri, projectName, actions, Path.GetTempPath()); //TODO replace temp path with something useful
        }
    }
}
