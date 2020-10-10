using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AutumnDudesSaveEditor
{
    class SaveFileParser
    {
        private static ActionElementMap ActionElementMapFromXElement(XElement actionElementMapXelement, XNamespace xNamespace)
        {
            ActionElementMap actionElementMap = new ActionElementMap()
            {
                ActionCategoryId = Int32.Parse(actionElementMapXelement.Element(xNamespace + "actionCategoryId").Value),
                ActionId = (ActionId)Int32.Parse(actionElementMapXelement.Element(xNamespace + "actionId").Value),
                ElementType = Int32.Parse(actionElementMapXelement.Element(xNamespace + "elementType").Value),
                ElementIdentifierId = (ElementIdentifierId)Int32.Parse(actionElementMapXelement.Element(xNamespace + "elementIdentifierId").Value),
                AxisRange = Int32.Parse(actionElementMapXelement.Element(xNamespace + "axisRange").Value),
                Invert = Boolean.Parse(actionElementMapXelement.Element(xNamespace + "invert").Value),
                AxisContribution = Int32.Parse(actionElementMapXelement.Element(xNamespace + "axisContribution").Value),
                KeyboardKeyCode = (KeyboardKeyCode)Int32.Parse(actionElementMapXelement.Element(xNamespace + "keyboardKeyCode").Value),
                ModifierKey1 = Int32.Parse(actionElementMapXelement.Element(xNamespace + "modifierKey1").Value),
                ModifierKey2 = Int32.Parse(actionElementMapXelement.Element(xNamespace + "modifierKey2").Value),
                ModifierKey3 = Int32.Parse(actionElementMapXelement.Element(xNamespace + "modifierKey3").Value),
                Enabled = Boolean.Parse(actionElementMapXelement.Element(xNamespace + "enabled").Value),
            };

            return actionElementMap;
        }

        private static XElement ActionElementMapToXElement(ActionElementMap actionElementMap, XNamespace xNamespace)
        {
            XElement actionElementMapXElem = new XElement(xNamespace + "ActionElementMap");
            actionElementMapXElem.Add(new XElement(xNamespace + "actionCategoryId", ((int)actionElementMap.ActionCategoryId).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "actionId", ((int)actionElementMap.ActionId).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "elementType", ((int)actionElementMap.ElementType).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "elementIdentifierId", ((int)actionElementMap.ElementIdentifierId).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "axisRange", ((int)actionElementMap.AxisRange).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "invert", Convert.ToBoolean(actionElementMap.Invert).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "axisContribution", ((int)actionElementMap.AxisContribution).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "keyboardKeyCode", ((int)actionElementMap.KeyboardKeyCode).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "modifierKey1", ((int)actionElementMap.ModifierKey1).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "modifierKey2", ((int)actionElementMap.ModifierKey2).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "modifierKey3", ((int)actionElementMap.ModifierKey3).ToString()));
            actionElementMapXElem.Add(new XElement(xNamespace + "enabled", Convert.ToBoolean(actionElementMap.Enabled).ToString()));

            return actionElementMapXElem;
        }

        public static SaveFile LoadFile(string pathToSaveFile)
        {
            SaveFile saveFile = new SaveFile() { FilePath = Path.GetFullPath(pathToSaveFile), Name = Path.GetFileName(pathToSaveFile) };

            if (Path.GetFileName(pathToSaveFile).StartsWith("FallGuysKeyBindings_0 playerName=Player1 dataType=ControllerMap controllerMapType=KeyboardMap"))
            {
                saveFile.Type = SaveType.KeyboardMap;
            }
            else if (Path.GetFileName(pathToSaveFile).StartsWith("FallGuysKeyBindings_0 playerName=Player1 dataType=ControllerMap controllerMapType=MouseMap"))
            {
                saveFile.Type = SaveType.MouseMap;
            }
            else
            {
                return null;
            }

            ObservableCollection<ActionElementMap> buttonMappings = new ObservableCollection<ActionElementMap>();
            ObservableCollection<ActionElementMap> axisMappings = new ObservableCollection<ActionElementMap>();

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { /*CheckCharacters = false,*/ IgnoreWhitespace = true, };
            using (StreamReader sr = new StreamReader(File.Open(pathToSaveFile, FileMode.Open)))
            {
                using (XmlReader xmlReader = XmlReader.Create(sr, xmlReaderSettings))
                {
                    xmlReader.MoveToContent();

                    XNamespace xNamespace = "http://guavaman.com/rewired";
                    XDocument xDocument = XDocument.Load(xmlReader);
                    XElement root = xDocument.Root;

                    //Get button maps
                    XElement buttonMaps = root.Elements(xNamespace + "buttonMaps").FirstOrDefault();
                    var actionElementMapsButtons = buttonMaps?.Elements(xNamespace + "ActionElementMap") ?? new List<XElement>();

                    foreach (XElement actionElementMapXElem in actionElementMapsButtons)
                    {
                        buttonMappings.Add(ActionElementMapFromXElement(actionElementMapXElem, xNamespace));
                    }

                    //Get axis maps
                    XElement axisMaps = root.Elements(xNamespace + "axisMaps").FirstOrDefault();
                    var actionElementMapsAxis = axisMaps?.Elements(xNamespace + "ActionElementMap") ?? new List<XElement>();

                    foreach (XElement actionElementMapXElem in actionElementMapsAxis)
                    {
                        axisMappings.Add(ActionElementMapFromXElement(actionElementMapXElem, xNamespace));
                    }
                }
            }

            saveFile.AxisMaps = axisMappings;
            saveFile.ButtonMaps = buttonMappings;

            return saveFile;
        }

        public static void SaveFile(SaveFile saveFile)
        {

            XNamespace xNamespace = "http://guavaman.com/rewired";
            XDocument xDocument = null;

            using (StreamReader sr = new StreamReader(File.Open(saveFile.FilePath, FileMode.Open), Encoding.UTF8))
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { /*CheckCharacters = false,*/ IgnoreWhitespace = true, };

                using (XmlReader xmlReader = XmlReader.Create(sr, xmlReaderSettings))
                {
                    xmlReader.MoveToContent();
                    xDocument = XDocument.Load(xmlReader);
                }

            }

            XElement root = xDocument.Root;
            XElement buttonMaps = root.Elements(xNamespace + "buttonMaps").FirstOrDefault();
            if (buttonMaps != null)
            {
                buttonMaps.Descendants().Remove();
                foreach (ActionElementMap actionElementMap in saveFile.ButtonMaps)
                {
                    buttonMaps.Add(ActionElementMapToXElement(actionElementMap, xNamespace));
                }
            }


            XElement axisMaps = root.Elements(xNamespace + "axisMaps").FirstOrDefault();
            if (axisMaps != null)
            {
                axisMaps.Descendants().Remove();
                foreach (ActionElementMap actionElementMap in saveFile.AxisMaps)
                {
                    axisMaps.Add(ActionElementMapToXElement(actionElementMap, xNamespace));
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(File.OpenWrite(saveFile.FilePath), Encoding.UTF8))
            {
                xDocument.Save(streamWriter, SaveOptions.None);
            }

        }
    }
}
