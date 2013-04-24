using System.Collections.Generic;
using System.Linq;
using WitMorph.Actions;

namespace WitMorph.Structures
{
    public class MorphActionSet
    {
        public MorphActionSet()
        {
            PrepareCollectionFieldDefinitions = new List<MorphAction>();
            PrepareWorkItemTypeDefinitions = new List<MorphAction>();
            ProcessWorkItemData = new List<MorphAction>();
            FinaliseWorkItemTypeDefinitions = new List<MorphAction>();
        }

        public ICollection<MorphAction> PrepareCollectionFieldDefinitions { get; private set; }

        public ICollection<MorphAction> PrepareWorkItemTypeDefinitions { get; private set; }

        public ICollection<MorphAction> ProcessWorkItemData { get; private set; }

        public ICollection<MorphAction> FinaliseWorkItemTypeDefinitions { get; private set; }

        public IEnumerable<MorphAction> Combine()
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