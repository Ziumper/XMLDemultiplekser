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
            if (pathToXMLFile.IsInitialized)
            {
                doc.Load(pathToXMLFile.Text);
                /*
                 * 1. Podlicz targety i stwórz listę ze stringami 
                 * 2. Stwórz dla każdego pola z targetem kategorie
                 * 3. Wpisz każde pole z targetem do noda z kategorią , jeżeli field nie ma targetu powinien on zostać 
                 * umieszczony w kategori głównej.
                 */
                List<string> categories = GetListOfCategories(doc);

                XmlNode tableNode = doc.SelectSingleNode("/table");

                XmlNodeList fieldLists = tableNode.SelectNodes("/field/");
                foreach(XmlNode fieldNode in fieldLists)
                {
                    bool isTarget = fieldNode.Attributes["target"].Value != null;
                    if(isTarget)
                    {
                        string target = fieldNode.Attributes["target"].Value;
                        bool isCategory = fieldNode.Attributes["type"].Value.Equals("category");

                        XmlNode categoryNode = GetCategoryFieldNodeByTarget(target, doc);

                        if (categoryNode == null)
                        {
                            categoryNode = CreateCategoryNode(target, doc);
                            XmlNode panelNode = CreatePanelNode("testCaption", target, 1, doc);
                            categoryNode.AppendChild(panelNode);
                        }

                        if (!isCategory)
                        {
                          
                            XmlNodeList panels = categoryNode.SelectNodes("/panel/");
                            XmlNode panel = panels.Item(1);
                            panel.AppendChild(fieldNode);
                        }
                    }
                }
               
               
                
            }else
            {
                MessageBox.Show("XML file is not loaded");
            }
        }

        private XmlNode GetCategoryFieldNodeByTarget(string target, XmlDocument doc) 
        {
            XmlNodeList xmlNodeList = doc.SelectNodes("/table/field/");
            
            foreach(XmlNode xmlNode in xmlNodeList)
            {
                bool haveTheSameTarget = xmlNode.Attributes["target"].Value.Equals(target);
                bool isCategory = xmlNode.Attributes["type"].Value.Equals("category");
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

        private List<string> GetListOfCategories(XmlDocument loadedDocument)
        {
            List<string> categories = new List<string>();
            XmlNodeList sectionNodes = loadedDocument.SelectNodes("/table/form/section");
            foreach(XmlNode node in sectionNodes)
            {
                XmlNodeList pagesNodes = node.SelectNodes("/pages/");
                foreach(XmlNode pageNode in pagesNodes)
                {
                    string target = pageNode.Attributes["name"].Value;
                    categories.Add(target);

                }
            }

            return categories;
        }
    }
}
