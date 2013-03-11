using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WitMorph.Differences;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    public class Convert_Agile_6_To_Scrum_2_Scenario
    {
        private static IEnumerable<IDifference> _differences;

        protected static IEnumerable<IDifference> Differences
        {
            get
            {
                if (_differences != null) return _differences;

                using (var currentTemplate = EmbeddedProcessTemplate.Agile6())
                using (var goalTemplate = EmbeddedProcessTemplate.Scrum2())
                {
                    var currentTemplateReader = new ProcessTemplateReader(currentTemplate.TemplatePath);
                    var goalTemplateReader = new ProcessTemplateReader(goalTemplate.TemplatePath);

                    var currentProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(currentTemplateReader.WorkItemTypeDefinitions.ToArray())};
                    var goalProcessTemplate = new ProcessTemplate {WorkItemTypeDefinitions = new ReadOnlyCollection<WorkItemTypeDefinition>(goalTemplateReader.WorkItemTypeDefinitions.ToArray())};

                    var diffEngine = new DiffEngine(ProcessTemplateMap.ConvertAgile6ToScrum2());
                    _differences = diffEngine.CompareProcessTemplates(currentProcessTemplate, goalProcessTemplate);
                }

                return _differences;
            }
        }
     
    }
}