using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace XMLDemultiplekser.OptionsXML
{
    public class OptionsParser
    {
        private string _pathToOriginalXmlFile;
        private string _pathToShared;

        public List<XmlNode> ListOfFieldsWithOptions { get; }

        public OptionsParser(string pathToOriginalXmlFile,string pathToShared) 
        {
            _pathToOriginalXmlFile = pathToOriginalXmlFile;
            _pathToShared = pathToShared;
            ListOfFieldsWithOptions = new List<XmlNode>();
        }

        public void CreateIncludeOptionFilesFromXmlFile()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(_pathToOriginalXmlFile);
                SetListOfFieldsWithOptions(doc);

                foreach (XmlNode fieldWithOptions in ListOfFieldsWithOptions)
                {
                    if(!IsOptionIsInShared(fieldWithOptions))
                    {
                       CreateIncludeOptionFile(fieldWithOptions);
                    }
                    CreateIncludeNodeForOptionNodeInSourceDocument(doc, fieldWithOptions);
                }

            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SaveXmlDocument(doc, _pathToOriginalXmlFile);
        }

        public void CreateInheritedOptionFilesFromXmlFile()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(_pathToOriginalXmlFile);
                SetListOfFieldsWithOptions(doc);

                foreach (XmlNode fieldWithOptions in ListOfFieldsWithOptions)
                {
                    if (!IsOptionIsInShared(fieldWithOptions))
                    {
                        CreateInheritedOptionfile(fieldWithOptions);
                    }
                    CreateInhereitedNodeForOptionNodeInSourceDocument(doc, fieldWithOptions);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SaveXmlDocument(doc, _pathToOriginalXmlFile);
        }

        private void CreateInheritedOptionfile(XmlNode fieldWithOptions)
        {
            XmlDocument optionDocument = new XmlDocument();

            XmlNode newFieldNode = optionDocument.ImportNode(fieldWithOptions, true);

            newFieldNode.Attributes.RemoveAll();
            XmlAttribute nameAttribute = optionDocument.CreateAttribute("name");
            nameAttribute.Value = "f1";
            newFieldNode.Attributes.Append(nameAttribute);

            optionDocument.AppendChild(newFieldNode);

            string pathToFile = GetPathToOptionsFile(fieldWithOptions);

            SaveXmlDocument(optionDocument, pathToFile);
        }

        private void CreateInhereitedNodeForOptionNodeInSourceDocument(XmlDocument doc,XmlNode fieldNode)
        {
            string optionFileName = GetOptionsFileName(fieldNode);
            XmlNodeList xmlNodeList = fieldNode.SelectNodes("option");
            foreach (XmlNode optionNodeChild in xmlNodeList)
            {
                fieldNode.RemoveChild(optionNodeChild);
            }

            InheriteOption(doc, fieldNode, optionFileName);
        }

        private void CreateIncludeNodeForOptionNodeInSourceDocument(XmlDocument doc, XmlNode fieldNode)
        {
            string optionFileName = GetOptionsFileName(fieldNode);

            XmlNodeList xmlNodeList = fieldNode.SelectNodes("option");
            foreach(XmlNode optionNodeChild in xmlNodeList)
            {
                fieldNode.RemoveChild(optionNodeChild);
            }

            IncludeOption(doc, fieldNode, optionFileName);
        }

        private void InheriteOption(XmlDocument doc, XmlNode fieldNode, string optionFileName)
        {
            XmlAttribute inheritedAttriubte = doc.CreateAttribute("inherited");
            inheritedAttriubte.Value = "../../shared/options/" + optionFileName;

            fieldNode.Attributes.Append(inheritedAttriubte);
        }

        private void IncludeOption(XmlDocument doc,XmlNode fieldNode,string optionFileName)
        {
            XmlNode includeNode = doc.CreateNode(XmlNodeType.Element, "include", "");
            includeNode.InnerText = "../../shared/options/" + optionFileName;

            fieldNode.AppendChild(includeNode);
        }

        private void CreateIncludeOptionFile(XmlNode fieldWithOptions)
        {
            XmlDocument optionDocument = new XmlDocument();
            XmlNode contentNode = GetContentNode(optionDocument);

            XmlNodeList optionsNodeList = fieldWithOptions.SelectNodes("option");
            foreach(XmlNode optionNode in optionsNodeList)
            {
                XmlNode newOptionNode = optionDocument.ImportNode(optionNode, true);
                contentNode.AppendChild(newOptionNode);
            }

            optionDocument.AppendChild(contentNode);

            string pathToFile = GetPathToOptionsFile(fieldWithOptions);

            SaveXmlDocument(optionDocument, pathToFile);


        }

        private void SaveXmlDocument(XmlDocument doc,string pathToFile)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(pathToFile, settings);
            doc.Save(writer);
        }
        
        private XmlNode GetContentNode(XmlDocument optionDocument)
        {
            XmlNode newContentNode = optionDocument.CreateNode(XmlNodeType.Element, "content", "");
            return newContentNode;
        }
        
        private void SetListOfFieldsWithOptions(XmlDocument doc)
        {
            XmlNode mainTableNode = doc.SelectSingleNode("table");
            XmlNodeList fieldList = mainTableNode.SelectNodes("field");
            FindFieldWithOptionsAndAddItToOptionFieldList(fieldList);
            fieldList = mainTableNode.SelectNodes("field/panel/field");
            FindFieldWithOptionsAndAddItToOptionFieldList(fieldList);
        }

        private void FindFieldWithOptionsAndAddItToOptionFieldList(XmlNodeList fieldList)
        {
            foreach (XmlNode fieldNode in fieldList)
            {
                XmlNodeList optionsNodes = fieldNode.SelectNodes("option");
                if (optionsNodes.Count > 0)
                {
                    ListOfFieldsWithOptions.Add(fieldNode);
                }
            }

        }

        private bool IsOptionIsInShared(XmlNode fieldWithOptions)
        {
            string filedname = fieldWithOptions.Attributes["name"].Value;
            if(filedname != null)
            {
                string pathToOptionsFile = GetPathToOptionsFile(filedname);
                return File.Exists(pathToOptionsFile);
            }
            return false;
        }

        private string GetPathToOptionsFile(string fieldname)
        {
            return _pathToShared + fieldname + "Options.xml";
        }

        private string GetPathToOptionsFile(XmlNode fieldWithOptions)
        {
            string pathToOptionsFile = "";
            string fieldName = fieldWithOptions.Attributes["name"].Value;
            if(fieldName != null)
            {
                pathToOptionsFile = GetPathToOptionsFile(fieldName);
            }

            return pathToOptionsFile;
        }

        private string GetOptionsFileName(XmlNode fieldWithOptions)
        {
            string optionsFileName = "";

            string fieldName = fieldWithOptions.Attributes["name"].Value;

            optionsFileName = fieldName + "Options.xml";

            return optionsFileName;
        }

        
    }
}
