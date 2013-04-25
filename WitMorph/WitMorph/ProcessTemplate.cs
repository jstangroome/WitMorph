using System.Collections.Generic;
using WitMorph.Model;

namespace WitMorph
{
    public class ProcessTemplate
    {
        public IReadOnlyList<WorkItemTypeDefinition> WorkItemTypeDefinitions { get; set; }
        // TODO work item queries
        // TODO work item link types
        // TODO work item categories
        // TODO agile and common configuration
        // TODO default areas and iterations
        // TODO reports
        // TODO test variables, test configurations, test resolution states, test settings?
        // TODO low pri - SharePoint Portal
        // TODO low pri - MSProject field Mappings
        // TODO permissions
    }
}