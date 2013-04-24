using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WitMorph.Actions;
using WitMorph.Model;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    public abstract class Convert_Scrum_2_To_Agile_6_Scenario 
    {
        private static List<MorphAction> _actionsViaDiffEngine;

        protected static List<MorphAction> GenerateActionsViaDiffEngine()
        {
            if (_actionsViaDiffEngine != null) return _actionsViaDiffEngine;

            using (var agileTemplate = EmbeddedProcessTemplate.Agile6())
            using (var scrumTemplate = EmbeddedProcessTemplate.Scrum2())
            {
                var agileReader = new ProcessTemplateReader(agileTemplate.TemplatePath);
                var scrumReader = new ProcessTemplateReader(scrumTemplate.TemplatePath);

                var processTemplateMap = ProcessTemplateMap.ConvertScrum2ToAgile6();

                var currentProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(scrumReader.WorkItemTypeDefinitions.ToArray())};
                var goalProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(agileReader.WorkItemTypeDefinitions.ToArray())};

                var diffEngine = new DiffEngine(processTemplateMap);
                var differences = diffEngine.CompareProcessTemplates(currentProcessTemplate, goalProcessTemplate);

                var morphEngine = new MorphEngine();
                _actionsViaDiffEngine = morphEngine.GenerateActions(differences).ToList();
            }
            
            return _actionsViaDiffEngine;
        }
    }
}