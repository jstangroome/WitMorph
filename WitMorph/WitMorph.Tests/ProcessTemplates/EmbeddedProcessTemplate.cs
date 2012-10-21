using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace WitMorph.Tests.ProcessTemplates
{
    public class EmbeddedProcessTemplate : IDisposable
    {
        public static EmbeddedProcessTemplate Agile6()
        {
            return new EmbeddedProcessTemplate("MSF for Agile Software Development 6.0");
        }

        public static EmbeddedProcessTemplate Scrum2()
        {
            return new EmbeddedProcessTemplate("Microsoft Visual Studio Scrum 2.0");
        }

        private string _templatePath;

        public EmbeddedProcessTemplate(string processTemplateName)
        {
            _templatePath = Path.GetTempFileName();
            File.Delete(_templatePath);

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), processTemplateName + ".zip"))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    archive.ExtractToDirectory(_templatePath);
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