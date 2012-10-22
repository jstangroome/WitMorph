using System;
using System.Collections.Generic;
using System.Xml;

namespace WitMorph.Actions
{
    public class LazyImportWorkItemTypeDefinitionMorphAction : IMorphAction
    {
        private readonly string _workItemTypeName;
        private readonly List<Action<ImportWorkItemTypeDefinitionMorphAction>> _actions = new List<Action<ImportWorkItemTypeDefinitionMorphAction>>();

        public LazyImportWorkItemTypeDefinitionMorphAction(string workItemTypeName)
        {
            _workItemTypeName = workItemTypeName;
        }

        // TODO consider using Expression-based parameter to extract action details
        public void AddSchemaChange(Action<IWorkItemTypeDefinitionSchemaChange> action)
        {
            _actions.Add(action);
        }

        public void Execute(ExecutionContext context)
        {
            if (_actions.Count == 0)
            {
                return;
            }

            var project = context.GetWorkItemProject();
            var witdElement = (XmlElement)project.WorkItemTypes[_workItemTypeName].Export(false).FirstChild;
            var importAction = new ImportWorkItemTypeDefinitionMorphAction(witdElement);

            foreach (var action in _actions)
            {
                action(importAction);
            }

            importAction.Execute(context);
        }

        public override string ToString()
        {
            if (_actions.Count == 0)
            {
                return string.Format("No action required. {0}", base.ToString());
            }
            return string.Format("Import {0} schema change(s) to work item type definition '{1}'", _actions.Count, _workItemTypeName); //TODO list details of changes
        }
    }

}