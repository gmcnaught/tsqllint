using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using TSQLLint.Common;
using TSQLLint.Lib.Parser.Interfaces;
using TSQLLint.Lib.Plugins;
using TSQLLint.Lib.Plugins.Interfaces;

namespace TSQLLint.Lib.Parser
{
    public class SqlFileProcessor : ISqlFileProcessor
    {
        private readonly IRuleVisitor _ruleVisitor;
        private readonly IReporter _reporter;
        private readonly IFileSystem _fileSystem;
        private readonly IPluginHandler _pluginHandler;
        
        public int FileCount { get; private set; }

        public SqlFileProcessor(IRuleVisitor ruleVisitor, IPluginHandler pluginHandler, IReporter reporter, IFileSystem fileSystem)
        {
            _ruleVisitor = ruleVisitor;
            _pluginHandler = pluginHandler;
            _reporter = reporter;
            _fileSystem = fileSystem;
        }

        public void ProcessList(List<string> paths)
        {
            foreach (var path in paths)
            {
                ProcessPath(path);
            }
        }

        private void ProcessFile(Stream fileStream, string filePath)
        {
            ProcessRules(fileStream, filePath);
            ProcessPlugins(fileStream, filePath);
            FileCount++;
        }

        public void ProcessPath(string path)
        {
            // remove quotes from path
            path = path.Replace("\"", "");

            var pathStrings = path.Split(',');
            for (var index = 0; index < pathStrings.Length; index++)
            {
                // remove leading and trailing whitespace
                pathStrings[index] = pathStrings[index].Trim();
            }

            foreach (var pathString in pathStrings)
            {
                if (!_fileSystem.File.Exists(pathString))
                {
                    if (_fileSystem.Directory.Exists(pathString))
                    {
                        ProcessDirectory(pathString);
                    }
                    else
                    {
                        ProcessWildCard(pathString);
                    }
                }
                else
                {
                    ProcessFile(GetFileContents(pathString), pathString);
                }
            }
        }

        private void ProcessDirectory(string path)
        {
            var subDirectories = _fileSystem.Directory.GetDirectories(path);
            foreach (var t in subDirectories)
            {
                ProcessPath(t);
            }

            var fileEntries = _fileSystem.Directory.GetFiles(path);
            foreach (var t in fileEntries)
            {
                ProcessIfSqlFile(t);
            }
        }

        private void ProcessIfSqlFile(string fileName)
        {
            if (_fileSystem.Path.GetExtension(fileName).Equals(".sql", StringComparison.InvariantCultureIgnoreCase))
            {
                ProcessFile(GetFileContents(fileName), fileName);
            }            
        }

        private void ProcessWildCard(string path)
        {
            var containsWildCard = path.Contains("*") || path.Contains("?");
            if (!containsWildCard)
            {
                _reporter.Report($"{path} is not a valid path.");
                return;
            }

            var dirPath = _fileSystem.Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dirPath))
            {
                dirPath = _fileSystem.Directory.GetCurrentDirectory();
            }

            if (!_fileSystem.Directory.Exists(dirPath))
            {
                _reporter.Report($"Directory does not exist: {dirPath}");
                return;
            }

            var searchPattern = _fileSystem.Path.GetFileName(path);
            var files = _fileSystem.Directory.EnumerateFiles(dirPath, searchPattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                ProcessIfSqlFile(file);
            }
        }

        private void ProcessRules(Stream fileStream, string filePath)
        {
            _ruleVisitor.VisitRules(filePath, fileStream);
        }

        private void ProcessPlugins(Stream fileStream, string filePath)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            TextReader textReader = new StreamReader(fileStream);
            _pluginHandler.ActivatePlugins(new PluginContext(filePath, textReader));
        }

        private Stream GetFileContents(string filePath)
        {
            return _fileSystem.File.OpenRead(filePath);
        }
    }
}
