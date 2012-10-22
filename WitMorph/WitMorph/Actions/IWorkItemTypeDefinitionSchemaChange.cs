using System.Xml;

namespace WitMorph.Actions
{
    public interface IWorkItemTypeDefinitionSchemaChange
    {
        void RemoveFieldDefinition(string fieldReferenceName);
        void RemoveWorkflowState(string state);
        void ReplaceFieldDefinition(string originalRefName, XmlElement newFieldElement);
        void ReplaceForm(XmlElement formElement);
        void ReplaceWorkflow(XmlElement workflowElement);
    }
}