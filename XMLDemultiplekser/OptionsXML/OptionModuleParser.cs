using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLDemultiplekser.OptionsXML
{
    public class OptionModuleParser
    {
        public List<string> ListOfTableFiles { get; }

        private string _pathToModule;

        public OptionModuleParser(string pathToModule)
        {
            _pathToModule = pathToModule;
            ListOfTableFiles = new List<string>();
        }

        public void ParseOptionsForModule()
        {
            SetTableFiles();

            

        }

        private void ParseOptionForTableFile(string filePath)
        {

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
            var prefix = "";
            for(var i = 0; i < 3; i++)
            {
                prefix += filePath[i];
            }

            return prefix == "dk_";
        }
    }
}
