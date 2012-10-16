using System;
using System.IO;
using System.IO.Compression;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;

namespace WitMorph
{
    public class ProcessTemplateDownloader : IDisposable
    {
        private string _templatePath;

        public ProcessTemplateDownloader(TfsTeamProjectCollection collection, string processTemplateName)
        {
            var processTemplates = collection.GetService<IProcessTemplates>();
            var index = processTemplates.GetTemplateIndex(processTemplateName);
            if (index < 0)
            {
                throw new ArgumentException("Process template not found.");
            }
            
            var templateFile = processTemplates.GetTemplateData(index);

            _templatePath = Path.GetTempFileName();
            File.Delete(_templatePath);

            ZipFile.ExtractToDirectory(templateFile, _templatePath);
            File.Delete(templateFile);
        }

        public string TemplatePath { get { return _templatePath; } }

        public void Dispose()
        {
            var templatePath = _templatePath;
            if (templatePath != null)
            {
                Directory.Delete(templatePath, recursive: true);
                _templatePath = null;
            }
        }
    }
}