using System.Collections.Generic;
using System.Linq;
using WitMorph.Actions;

namespace WitMorph.Structures
{
    public class MorphActionSet
    {
        public MorphActionSet()
        {
            PrepareCollectionFieldDefinitions = new List<IMorphAction>();
            PrepareWorkItemTypeDefinitions = new List<IMorphAction>();
            ProcessWorkItemData = new List<IMorphAction>();
            FinaliseWorkItemTypeDefinitions = new List<IMorphAction>();
        }

        public ICollection<IMorphAction> PrepareCollectionFieldDefinitions { get; private set; }

        public ICollection<IMorphAction> PrepareWorkItemTypeDefinitions { get; private set; }

        public ICollection<IMorphAction> ProcessWorkItemData { get; private set; }

        public ICollection<IMorphAction> FinaliseWorkItemTypeDefinitions { get; private set; }

        public IEnumerable<IMorphAction> Combine()
        {
            var collections = new[]
                              {
                                  PrepareCollectionFieldDefinitions,
                                  PrepareWorkItemTypeDefinitions,
                                  ProcessWorkItemData,
                                  FinaliseWorkItemTypeDefinitions
                              };
            return collections.SelectMany(collection => collection);
        }
    }
}