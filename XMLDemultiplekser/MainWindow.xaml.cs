using Microsoft.Win32;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Xml;
using System.IO;
using XMLDemultiplekser.OptionsXML;

namespace XMLDemultiplekser
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadXMLFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? canOpen = ofd.ShowDialog();
            if(canOpen == true)
            {
                SetPathToXmlFile(ofd);
            }
        }

        private void CreateCategories(object sender,RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(pathToXMLFile.Text);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                Dictionary<string, string> categories = GetListOfCategories(doc);

                XmlNode tableNode = doc.SelectSingleNode("table");

                XmlNodeList fieldLists = tableNode.SelectNodes("field");
                foreach (XmlNode fieldNode in fieldLists)
                {
                    AppendFieldToCategory(fieldNode, doc, tableNode, categories);
                }

                List<XmlNode> categoryNodes = GetCategoriesFields(tableNode);

                MoveCategoriesNodeOnTheBegining(categoryNodes, tableNode);
                SetShowtitleAttribute(categories, tableNode,categoryNodes,doc);

                doc.Save(pathToXMLFile.Text);

                MessageBox.Show("Categories created succesfully!");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error occured:\n" + ex.Message + "\n StackTrace:" +  ex.StackTrace);
            }
           


        }

        private void SetShowtitleAttribute(Dictionary<string, string> categories, XmlNode tableNode,List<XmlNode> categoryNodes,XmlDocument document)
        {
            List<string> pagesNamesWithTargets = new List<string>();

            foreach (XmlNode categoryNode in categoryNodes)
            {
                var target = categoryNode.Attributes["target"].Value;
                pagesNamesWithTargets.Add(target);
            }

            XmlNodeList sectionNodes = tableNode.SelectNodes("form/section");

            foreach(XmlNode section in sectionNodes)
            {
                XmlNodeList pageNodes = section.SelectNodes("page");

                foreach (XmlNode pageNode in pageNodes)
                {
                    string pageName = pageNode.Attributes["name"].Value;
                    bool haveCategory = pagesNamesWithTargets.Contains(pageName);
                    if (haveCategory)
                    {
                        XmlAttribute pageShowTitleAttribute = pageNode.Attributes["showtitle"];
                        if (pageShowTitleAttribute == null)
                        {
                            pageShowTitleAttribute = document.CreateAttribute("showtitle");
                        }

                        pageShowTitleAttribute.Value = "false";
                    }
                }
            }

            

        }

        private void AppendFieldToCategory(XmlNode fieldNode,XmlDocument doc, XmlNode tableNode,Dictionary<string,string> categories)
        {
            bool haveTarget = fieldNode.Attributes["target"]?.Value != null;
            if (haveTarget)
            {
                AppendFieldWithTargetToCategory(fieldNode, doc, categories, tableNode);
            }
            else
            {
                AppendFieldWithoutTargetToCategory(categories, doc, tableNode, fieldNode);
            }
        }

        private void AppendFieldWithoutTargetToCategory(Dictionary<string,string> categories,XmlDocument doc,XmlNode tableNode, XmlNode fieldNode)
        {
            string firstTarget = categories.Keys.First();
            XmlNode categoryNode = GetCategoryFieldNodeByTarget(firstTarget, doc);

            if (categoryNode == null)
            {
                categoryNode = AppendCategoryToTable(firstTarget, categories[firstTarget], doc, tableNode);
            }

            AppendFieldToPanel(firstTarget, categories[firstTarget], categoryNode, doc, tableNode, fieldNode);

        }

        private void AppendFieldWithTargetToCategory(XmlNode fieldNode,XmlDocument doc,Dictionary<string,string> categories,XmlNode tableNode)
        {
            string target = fieldNode.Attributes["target"].Value;

            string categoryValue = fieldNode.Attributes["type"].Value;
            bool isCategory = categoryValue != null && categoryValue.Equals("category");
            XmlNode categoryNode = GetCategoryFieldNodeByTarget(target, doc);

            if (categoryNode == null)
            {
                if (!categories.Keys.Contains(target))
                {
                    categories.Add(target, target);   
                }

                categoryNode = AppendCategoryToTable(target, categories[target], doc, tableNode);
            }

            if (!isCategory)
            {
                AppendFieldToPanel(target, categories[target], categoryNode, doc, tableNode, fieldNode);
            }
        }

        private XmlNode AppendCategoryToTable(string target,string caption,XmlDocument doc,XmlNode tableNode)
        {
            XmlNode categoryNode = CreateCategoryNode(target, doc);
            int index = 0;
            XmlNode firstPanelNode = CreatePanelNode(caption, target, index, doc);
            categoryNode.AppendChild(firstPanelNode);
            tableNode.AppendChild(categoryNode);
            return categoryNode;
        }

        private void AppendFieldToPanel(string target,string caption,XmlNode categoryNode,XmlDocument doc, XmlNode tableNode,XmlNode fieldNode)
        {
            XmlNodeList panels = categoryNode.SelectNodes("panel");
            if (panels == null)
            {
                int index = 0;
                var firstPanel = CreatePanelNode(caption, target, index, doc);
                categoryNode.AppendChild(firstPanel);
                panels = categoryNode.SelectNodes("panel");
            }

            XmlAttribute mTypeAttribute = fieldNode.Attributes["mtype"];
            if(mTypeAttribute != null)
            {
                bool isLabel = mTypeAttribute.Value.Equals("biglabel");
                if(isLabel)
                {
                    string panelCaption = fieldNode.Attributes["caption"].Value;
                    int index = categoryNode.SelectNodes("panel").Count;
                    XmlNode labelPanel = CreatePanelNode(panelCaption, target, index, doc);
                    categoryNode.AppendChild(labelPanel);
                    tableNode.RemoveChild(fieldNode);
                    return;
                }
            }
            
            XmlNode panel = panels.Item(panels.Count - 1);
            RemoveTargetAttribute(fieldNode);
            tableNode.RemoveChild(fieldNode);
            panel.AppendChild(fieldNode);
            
        }

        
        private void RemoveTargetAttribute(XmlNode fieldNode)
        {
            XmlAttribute targetAttribute = fieldNode.Attributes["target"];
            if (targetAttribute != null)
            {
                fieldNode.Attributes.Remove(targetAttribute);
            }
        }

        private void MoveCategoriesNodeOnTheBegining(List<XmlNode> categoryNodes, XmlNode tableNode)
        {
            var formNode = tableNode.SelectSingleNode("form");

            foreach (XmlNode node in categoryNodes)
            {
                tableNode.RemoveChild(node);
                tableNode.InsertBefore(node, formNode);
            }

        }



        private List<XmlNode> GetCategoriesFields(XmlNode tableNode)
        {
            List<XmlNode> listOfXmlNodes = new List<XmlNode>();
            XmlNodeList nodes = tableNode.SelectNodes("field");
            foreach(XmlNode node in nodes)
            {
                string typeCategoryValue = node.Attributes["type"]?.Value;
                bool isCategory = typeCategoryValue != null && typeCategoryValue.Equals("category");
                if(isCategory)
                {
                    listOfXmlNodes.Add(node);   
                }
            }

            return listOfXmlNodes;
        }

        private XmlNode GetCategoryFieldNodeByTarget(string target, XmlDocument doc) 
        {
            XmlNodeList xmlNodeList = doc.SelectNodes("/table/field");
            
            foreach(XmlNode xmlNode in xmlNodeList)
            {
                string targetValue = xmlNode.Attributes["target"]?.Value;
                bool haveTheSameTarget = targetValue != null && targetValue.Equals(target);
                string categoryValue = xmlNode.Attributes["type"]?.Value;
                bool isCategory =  categoryValue != null && categoryValue.Equals("category");
                if (haveTheSameTarget && isCategory)
                {
                    return xmlNode;
                }
            }

            return null; 
        }

        private XmlNode CreatePanelNode(string panelCaption,string categoryName,int index, XmlDocument doc)
        {
            XmlNode panelNode = doc.CreateNode(XmlNodeType.Element, "panel", "");

            XmlAttribute panelNameAttribute = doc.CreateAttribute("name");
            panelNameAttribute.Value = "mod_" + categoryName + index;

            XmlAttribute panelCaptionAttribute = doc.CreateAttribute("caption");
            panelCaptionAttribute.Value = panelCaption;

            panelNode.Attributes.Append(panelNameAttribute);
            panelNode.Attributes.Append(panelCaptionAttribute);

            return panelNode;
        }

        private XmlNode CreateCategoryNode(string categoryName,XmlDocument tableDocument)
        {
            XmlNode categoryNode = tableDocument.CreateNode(XmlNodeType.Element, "field", "");

            XmlAttribute categoryNameAttribute = tableDocument.CreateAttribute("name");
            categoryNameAttribute.Value = "kategoria_" + categoryName;

            XmlAttribute categoryType = tableDocument.CreateAttribute("type");
            categoryType.Value = "category";

            XmlAttribute categoryTarget = tableDocument.CreateAttribute("target");
            categoryTarget.Value = categoryName;

            categoryNode.Attributes.Append(categoryNameAttribute);
            categoryNode.Attributes.Append(categoryType);
            categoryNode.Attributes.Append(categoryTarget);

            
           
            return categoryNode;
        }

        private void SetPathToXmlFile(OpenFileDialog ofd)
        {
            pathToXMLFile.Text = ofd.FileName;
        }

        private Dictionary<string,string> GetListOfCategories(XmlDocument loadedDocument)
        {
            Dictionary<string, string> categories = new Dictionary<string, string>();
            XmlNodeList sectionNodes = loadedDocument.SelectNodes("/table/form/section");
            foreach(XmlNode node in sectionNodes)
            {
                XmlNodeList pagesNodes = node.SelectNodes("page");
                foreach(XmlNode pageNode in pagesNodes)
                {
                    string target = pageNode.Attributes["name"]?.Value;
                    string caption = pageNode.Attributes["caption"]?.Value;
                 
                    if (!categories.Keys.Contains(target))
                    {
                        categories.Add(target, caption);
                    }
                    
                }
            }

            return categories;
        }

        private void NameShowtables(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(pathToXMLFile.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {

                Dictionary<string,int> names = new Dictionary<string, int>();
                XmlNodeList showtableNodes = doc.SelectNodes("/table/showtable");
                

                foreach (XmlNode showtableNode in showtableNodes)
                {
                    XmlNode queryNode = showtableNode.SelectSingleNode("query");
                    string table = queryNode.Attributes["table"]?.Value;
                    bool isUnique = !names.Keys.Contains(table);
                    if(isUnique)
                    {
                        names.Add(table, 0);
                    }else
                    {
                        int index = 0;
                        names.TryGetValue(table, out index);

                        index++;
                        table = table + index.ToString();

                        names.Remove(table);

                        names.Add(table, index);
                    }

                    XmlAttribute nameAttribute = showtableNode.Attributes["name"];

                    if(nameAttribute != null)
                    {
                        nameAttribute.Value = table;
                    }
                    else
                    {
                        nameAttribute = doc.CreateAttribute("name");
                        nameAttribute.Value = table;
                        showtableNode.Attributes.Append(nameAttribute);
                    }
                   
                }

                doc.Save(pathToXMLFile.Text);

                MessageBox.Show("Names assigned successfuly!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured:\n" + ex.Message + "\n StackTrace:" + ex.StackTrace);
            }
        }

        private void AddInherited(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(pathToXMLFile.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                XmlNodeList showtableNodes = doc.SelectNodes("/table/showtable");


                foreach (XmlNode showtableNode in showtableNodes)
                {
                    XmlNode queryNode = showtableNode.SelectSingleNode("query");
                    string table = queryNode.Attributes["table"]?.Value;
                    string path = pathToInherited.Text + table + ".xml";
                    
                   
                    XmlAttribute nameAttribute = showtableNode.Attributes["inherited"];
                    if (nameAttribute != null)
                    {
                        nameAttribute.Value = path;
                    }
                    else
                    {
                        nameAttribute = doc.CreateAttribute("inherited");
                        nameAttribute.Value = path;
                        showtableNode.Attributes.Append(nameAttribute);
                    }

                }

                doc.Save(pathToXMLFile.Text);

                MessageBox.Show("Inherited assigned successfuly!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured:\n" + ex.Message + "\n StackTrace:" + ex.StackTrace);
            }
        }

        private void CreateXmlFiles(object sender, RoutedEventArgs e)
        {
          
            try
            {
                XmlDocument configDocument = new XmlDocument();
                string root = Path.GetDirectoryName(pathToXMLFile.Text);
                CreateIncludeFolder(root);
                configDocument.Load(pathToXMLFile.Text);
                CreateIncludedFiles(configDocument, root, "module/tables/table", "module/tables", "name");
                CreateIncludedFiles(configDocument, root, "module/list_tables/list", "module/list_tables", "caption");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

      
        }

        private void CreateIncludedFiles(XmlDocument configDocument, string root,string pathToListNodes,string pathToParent,string attributeKey)
        {
            XmlNodeList nodeList = configDocument.SelectNodes(pathToListNodes);
            XmlNode parentNode = configDocument.SelectSingleNode(pathToParent);

            int index = 0;

            foreach(XmlNode node in nodeList)
            {
                string includedPath = CreateIncludeFile(node, root, attributeKey,index);
                index++;
                parentNode.RemoveChild(node);
                XmlNode includeNode = configDocument.CreateNode(XmlNodeType.Element, "include", "");
                includeNode.InnerText = includedPath;
                parentNode.AppendChild(includeNode);
            }

            configDocument.Save(pathToXMLFile.Text);
        }

        private string CreateIncludeFile(XmlNode tableNode, string root, string attributeKey, int index)
        {

            XmlAttribute nameAttribute = tableNode.Attributes[attributeKey];
            if (nameAttribute != null)
            {
                string fileName = nameAttribute.Value;
              

                string fullPath = root + "\\inc\\" + fileName + ".xml";
                if (File.Exists(fullPath))
                {
                    XmlAttribute idnameAttribute = tableNode.Attributes["idname"];
                    if (idnameAttribute != null)
                    {
                        fileName = fileName + "_" + idnameAttribute.Value;
                        fullPath = root + "\\inc\\" + fileName + ".xml";
                    }
                    else
                    {
                        fileName = fileName + index.ToString();
                        fullPath = fullPath = root + "\\inc\\" + fileName + ".xml";
                    }
                }


                XmlDocument tableDocument = new XmlDocument();
                XmlNode newTableNode = tableDocument.ImportNode(tableNode,true);
                tableDocument.AppendChild(newTableNode);

              
                tableDocument.Save(fullPath);

                string includePath = "inc/" + fileName + ".xml";

                return includePath;
            }

            return null;
        }

        private void CreateIncludeFolder(string root)
        {
            string subPath = "inc";
            string fullPath = root + "/" + subPath;
            bool exist = Directory.Exists(fullPath);
            if (!exist)
            {
                Directory.CreateDirectory(fullPath);
            }
        }


        private void RemovePanelsAndCreateLabels(string path)
        {
            /*
            1.Wczytaj wszystkie pliki xml z danego folderu
            2.Usuń panele oraz stwórz labele.
                2.0 - Nadaj targety wszystkim fieldom z kategorii
                    2.1.1 - Sprawdzić czy dany panel ma inherited jeżeli ma:
                        2.1.1.1 - pominąć i lecieć dalej lub, odczytać ścieżkę z inherita, wczytać pola i dorzucić.
                2.1 - Panele zastąp labelami.
                2.2 - Przenieś wszystkie fieldy poza kategorie
                2.3 - Usuń panel
                2.4 - Usuń kategorie.
            3.Ciesz się z dobrze wykonanej pracy!
            */

            //Na razie tylko jeden plik
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            XmlNode tableNode = doc.SelectSingleNode("table");
            List<XmlNode> categories = GetCategoriesFields(tableNode);

            foreach (var category in categories)
            {
                XmlAttribute targetAttribute = category.Attributes["target"];

                bool inheritedInPanel = false;
                XmlNodeList xmlPanels = category.SelectNodes("panel");


                foreach (XmlNode xmlPanelNode in xmlPanels)
                {
                    //sprawdź czy panel ma inherited 
                    XmlAttribute inherited = xmlPanelNode.Attributes["inherited"];
                    if (inherited != null)
                    {
                        inheritedInPanel = true;
                    }else
                    {
                        //tworzę label i wyrzucam poza kategorię
                        XmlNode labelField = doc.CreateNode(XmlNodeType.Element, "field", "");

                        //Tworzę attrubyty do labela
                        XmlAttribute nameAttribute = xmlPanelNode.Attributes["name"];
                        XmlAttribute typeAttribute = doc.CreateAttribute("type");
                        typeAttribute.Value = "empty";
                        XmlAttribute mTypeAttribute = doc.CreateAttribute("mtype");
                        mTypeAttribute.Value = "biglabel";
                        XmlAttribute captionAttribute = xmlPanelNode.Attributes["caption"];
                        XmlAttribute heightAttribute = doc.CreateAttribute("height");
                        heightAttribute.Value = "20";
                        XmlAttribute anchorBottomAttribute = doc.CreateAttribute("anchor_bottom");
                        anchorBottomAttribute.Value = "false";
                        XmlAttribute widthAttribute = doc.CreateAttribute("width");
                        widthAttribute.Value = "407";
                        XmlAttribute leftAttribute = doc.CreateAttribute("left");
                        leftAttribute.Value = "0";

                        XmlAttribute labelTargetAttribute = doc.CreateAttribute("target");
                        labelTargetAttribute.Value = targetAttribute.Value;

                        //dodaje atrybuty do labela
                        labelField.Attributes.Append(nameAttribute);
                        labelField.Attributes.Append(typeAttribute);
                        labelField.Attributes.Append(mTypeAttribute);
                        labelField.Attributes.Append(captionAttribute);
                        labelField.Attributes.Append(heightAttribute);
                        labelField.Attributes.Append(anchorBottomAttribute);
                        labelField.Attributes.Append(widthAttribute);
                        labelField.Attributes.Append(leftAttribute);

                        //dodaje target dla labela
                        labelField.Attributes.Append(labelTargetAttribute);

                        //przerzucam go do node dziadka czyli do tabelki, przed formem
                        XmlNode formNode = tableNode.SelectSingleNode("form");
                        tableNode.InsertBefore(labelField, formNode);

                        XmlNodeList fieldsNodes = xmlPanelNode.SelectNodes("field");

                        foreach (XmlNode xmlFiledsNode in fieldsNodes)
                        {
                            //Nadaje target
                            XmlAttribute fieldTargetAttribute = doc.CreateAttribute("target");
                            fieldTargetAttribute.Value = targetAttribute.Value;
                            xmlFiledsNode.Attributes.Append(fieldTargetAttribute);

                            //przerzucam przed form
                            tableNode.InsertBefore(xmlFiledsNode, formNode);

                            //Gotowe!
                        }

                        //kasuje panel
                        category.RemoveChild(xmlPanelNode);
                    }

                   
                }

                if (!inheritedInPanel)
                {
                    //Do usunięcią w fazie gdy będę szczytywał już informacje z inherita
                    //jeżeli nie było panelu inherita skasuj kategorię
                    tableNode.RemoveChild(category);
                }
            }

            doc.Save(path);

            MessageBox.Show("Panels removed succesfully!");
        }

        private async void RemovePanelsAndCreateLabels(object sender, RoutedEventArgs e)
        {
            string path = pathToXMLFile.Text;
            await Task.Run(() => {
                RemovePanelsAndCreateLabels(path);
            });
        }

        private void CreateOptionsFile(object sender, RoutedEventArgs e)
        {
            OptionModuleParser optionModuleParser = new OptionModuleParser(pathToModule.Text, pathToInherited.Text);
            optionModuleParser.ParseInheritedOptionsForModule();
            //OptionsParser optionsParser = new OptionsParser(pathToXMLFile.Text, pathToInherited.Text);
            //optionsParser.CreateInheritedOptionFilesFromXmlFile();
        }

        private void LoadPathToSharedFolder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? canOpen = ofd.ShowDialog();
            if (canOpen == true)
            {
                pathToInherited.Text = ofd.FileName;
            }
        }

        private void LoadPathToModuleFolder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? canOpen = ofd.ShowDialog();
            if (canOpen == true)
            {
                pathToModule.Text = ofd.FileName;
            }
        }

        private void CreateIncludeOptionsFile(object sender, RoutedEventArgs e)
        {
            OptionModuleParser optionModuleParser = new OptionModuleParser(pathToModule.Text, pathToInherited.Text);
            optionModuleParser.ParseIncludeOptionsForModule();
        }
    }
}
