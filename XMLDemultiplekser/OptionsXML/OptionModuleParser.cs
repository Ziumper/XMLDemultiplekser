using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XMLDemultiplekser.OptionsXML
{
    public class OptionModuleParser
    {
        public List<string> ListOfTableFiles { get; }

        private string _pathToModule;
        private string _pathToShared;

        public OptionModuleParser(string pathToModule, string pathToShared)
        {
            _pathToModule = pathToModule;
            _pathToShared = pathToShared;
            ListOfTableFiles = new List<string>();
        }

        public void ParseIncludeOptionsForModule()
        {
            SetTableFiles();

            foreach(string pathToTable in ListOfTableFiles)
            {
                ParseOptionForTableFile(pathToTable);
            }

        }

        public void ParseInheritedOptionsForModule()
        {
            SetTableFiles();

            foreach(string pathToTable in ListOfTableFiles)
            {
                OptionsParser optionsParser = new OptionsParser(pathToTable, _pathToShared);
                optionsParser.CreateInheritedOptionFilesFromXmlFile();
            }
        } 

        private void ParseOptionForTableFile(string filePath)
        {
            OptionsParser optionsParser = new OptionsParser(filePath, _pathToShared);
            optionsParser.CreateIncludeOptionFilesFromXmlFile();
        }

        private void SetTableFiles()
        {
            List<string> fillesInc = Directory.GetFiles(_pathToModule + "\\inc").ToList();
            foreach (string filePath in fillesInc)
            {
                if (isTable(filePath))
                {
                    ListOfTableFiles.Add(filePath);
                }
            }
        }

        private bool isTable(string filePath)
        {
            Regex regex = new Regex(@"\\(dk_)");
            var match = regex.Match(filePath);
            return match.Success;
        }
    }
}
