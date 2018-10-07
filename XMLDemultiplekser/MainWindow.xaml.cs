using Microsoft.Win32;
using System;
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
using System.Windows.Shapes;
using System.Xml;

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

                XmlNode tableNode = doc.SelectSingleNode("/table");

                XmlNodeList fieldLists = tableNode.SelectNodes("field");
                foreach (XmlNode fieldNode in fieldLists)
                {
                    AppendFieldToCategory(fieldNode, doc, tableNode, categories);
                }

                List<XmlNode> categoryNodes = GetCategoriesFields(tableNode);

                MoveCategoriesNodeOnTheBegining(categoryNodes, tableNode);

                doc.Save(pathToXMLFile.Text);

                MessageBox.Show("Categories created succesfully!");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error occured:\n" + ex.Message + "\n StackTrace:" +  ex.StackTrace);
            }
           


        }

        private void AppendFieldToCategory(XmlNode fieldNode,XmlDocument doc, XmlNode tableNode,Dictionary<string,string> categories)
        {
            bool isTarget = fieldNode.Attributes["target"]?.Value != null;
            if (isTarget)
            {
                string target = fieldNode.Attributes["target"].Value;
                string categoryValue = fieldNode.Attributes["type"].Value;

                bool isCategory = categoryValue != null && categoryValue.Equals("category");

                XmlNode categoryNode = GetCategoryFieldNodeByTarget(target, doc);

                if (categoryNode == null)
                {
                    categoryNode = AppendCategoryToTable(target,categories[target], doc, tableNode);
                }

                if (!isCategory)
                {
                    AppendFieldToPanel(target,categories[target], categoryNode, doc, tableNode, fieldNode);
                }
            }
            else
            {
                string firstTarget = categories.Keys.First();
                XmlNode categoryNode = GetCategoryFieldNodeByTarget(firstTarget, doc);

                if (categoryNode == null)
                {
                    categoryNode = AppendCategoryToTable(firstTarget,categories[firstTarget], doc, tableNode);
                }

                AppendFieldToPanel(firstTarget, categories[firstTarget], categoryNode, doc, tableNode, fieldNode);

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
            var firstNode = tableNode.ChildNodes.Item(0);

            string type = firstNode.Attributes["type"]?.Value;
            if (type != null)
            {
                if (type.Equals("category"))
                {
                    return;
                }
            }

            foreach (XmlNode node in categoryNodes)
            {
                tableNode.RemoveChild(node);
                tableNode.InsertBefore(node,firstNode);
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
                        XmlAttribute showtitleAttribute = pageNode.Attributes["showtitle"];
                        if (showtitleAttribute == null)
                        {
                            showtitleAttribute = loadedDocument.CreateAttribute("showtitle");
                            showtitleAttribute.Value = "false";
                            pageNode.Attributes.Append(showtitleAttribute);
                        }
                        showtitleAttribute.Value = "false";
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
    }
}
