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

        // TODO rename this method
        // TODO consider using Expression-based parameter to extract action details
        public void AddImportStep(Action<ImportWorkItemTypeDefinitionMorphAction> action)
        {
            _actions.Add(action);
        }

        public void Execute(ExecutionContext context)
        {
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
            return string.Format("Lazily import work item type definition '{0}'", _workItemTypeName); //TODO list details of changes
        }
    }

}