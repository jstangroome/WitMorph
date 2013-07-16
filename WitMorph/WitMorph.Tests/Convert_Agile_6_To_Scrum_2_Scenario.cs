using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WitMorph.Actions;
using WitMorph.Differences;
using WitMorph.Model;
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

                    var diffEngine = new DiffEngine(ProcessTemplateMaps.Agile61ToScrum21());
                    _differences = diffEngine.CompareProcessTemplates(currentProcessTemplate, goalProcessTemplate);
                }

                return _differences;
            }
        }

        private static MorphAction[] _actions;

        protected static MorphAction[] Actions
        {
            get
            {
                if (_actions != null) return _actions;

                var morphEngine = new MorphEngine();
                _actions = morphEngine.GenerateActions(Differences);

                return _actions;
            }
        }

    }
}