using System.Reflection;

namespace WitMorph
{
    public static class ProcessTemplateMaps
    {
        private static ProcessTemplateMap ReadEmbeddedResource(string baseName)
        {
            var fullName = string.Concat("WitMorph.ProcessTemplateMaps.", baseName, ".witmap");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName))
                return ProcessTemplateMap.Read(stream);
        }
        
        public static ProcessTemplateMap Agile61ToScrum21()
        {
            return ReadEmbeddedResource("Agile6.1_to_Scrum2.1");
        }
    }
}
