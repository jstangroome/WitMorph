using System.Collections.Generic;
using System.Linq;
using System.Xml;
using WitMorph.Differences;

namespace WitMorph.IntegrationTests
{
    public class ReportBuilder {
        public XmlDocument WriteDifferencesToXml(IEnumerable<IDifference> finalDifferences)
        {
            var diffArray = finalDifferences.ToArray();

            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("differences"));

            var groupedWorkItemDifferences = diffArray.OfType<IWorkItemTypeDifference>().GroupBy(x => x.CurrentWorkItemTypeName);

            foreach (var workItemGroup in groupedWorkItemDifferences)
            {
                var workItemElement = doc.DocumentElement.AppendElement(doc.CreateElement("workitem"));
                workItemElement.SetAttribute("name", workItemGroup.Key);

                var fieldsElement = workItemElement.AppendElement(doc.CreateElement("fields"));
                var workflowElement = workItemElement.AppendElement(doc.CreateElement("workflow"));

                foreach (var difference in workItemGroup)
                {
                    if (difference is ChangedWorkItemFieldDifference)
                    {
                        var change = difference as ChangedWorkItemFieldDifference;
                        var diffElement = fieldsElement.AppendElement(doc.CreateElement("changedfield"));
                        diffElement.SetAttribute("currentrefname", change.CurrentFieldReferenceName);
                    }
                    else if (difference is ChangedWorkItemStateDifference)
                    {
                        var change = difference as ChangedWorkItemStateDifference;
                        var diffElement = fieldsElement.AppendElement(doc.CreateElement("changedstate"));
                        diffElement.SetAttribute("currentstatename", change.CurrentStateName);
                        diffElement.SetAttribute("goalstatename", change.GoalStateName);
                    }
                    else if (difference is ChangedWorkItemWorkflowDifference)
                    {
                        var change = difference as ChangedWorkItemWorkflowDifference;
                        var diffElement = workflowElement.AppendElement(doc.CreateElement("changedworkflow"));
                        //diffElement.SetAttribute("currentrefname", change.WorkflowElement);
                    }
                    else if (difference is RemovedWorkItemFieldDifference)
                    {
                        var removed = difference as RemovedWorkItemFieldDifference;
                        var diffElement = fieldsElement.AppendElement(doc.CreateElement("removedfield"));
                        diffElement.SetAttribute("refname", removed.ReferenceFieldName);
                    }
                    else
                    {
                        workItemElement.AppendElement(doc.CreateElement("otherdifference")).SetAttribute("type", difference.GetType().Name);
                    }

                    // field

                    // state

                    // workflow

                    // form
                }

            }

            return doc;
        }
    }

    public static class XmlExtensions
    {
        public static XmlElement AppendElement(this XmlElement parentElement, XmlElement childElement)
        {
            return (XmlElement)parentElement.AppendChild(childElement);
        }
    }
}