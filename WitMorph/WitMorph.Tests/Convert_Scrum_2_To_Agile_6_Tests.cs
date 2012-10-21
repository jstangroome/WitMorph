using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WitMorph.Actions;
using WitMorph.Structures;
using WitMorph.Tests.ProcessTemplates;

namespace WitMorph.Tests
{
    [TestClass]
    public class Convert_Scrum_2_To_Agile_6_Tests
    {
        [TestMethod]
        public void ScrumToAgile_should_rename_PBI_to_User_Story()
        {
            using (var agileTemplate = EmbeddedProcessTemplate.Agile6())
            using (var scrumTemplate = EmbeddedProcessTemplate.Scrum2())
            {
                var agileReader = new ProcessTemplateReader(agileTemplate.TemplatePath);
                var scrumReader = new ProcessTemplateReader(scrumTemplate.TemplatePath);

                var processTemplateMap = new ProcessTemplateMap();
                var actionSet = new MorphActionSet();

                var sut = new WitdCollectionComparer(processTemplateMap, actionSet);
                sut.Compare(agileReader.WorkItemTypeDefinitions, scrumReader.WorkItemTypeDefinitions);

                var renameAction = actionSet.Combine()
                    .OfType<RenameWitdMorphAction>()
                    .SingleOrDefault(a => a.TypeName == "Product Backlog Item" && a.NewName == "User Story");

                Assert.IsNotNull(renameAction);
            }
            
        }
    }
}
