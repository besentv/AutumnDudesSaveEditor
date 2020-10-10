using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RegistryBinaryEditor
{
    public class Dump
    {
        /// <summary>
        /// Writes all files described in "RegMappings.xml" to the registry in HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="keyName">Registry key to write the values to.</param>
        /// <param name="dir">Directory in which the value files and "RegMappings.xml" file is stored.</param>
        public static void WriteHKCUBinaryValuesFromFiles(string keyName, string dir)
        {
            string fileDirPath = Path.GetFullPath(dir);

            var xDocument = XDocument.Load($"{fileDirPath}/RegMappings.xml");
            XElement mappings = xDocument.Element("mappings");

            using (RegistryKey keyToWrite = Registry.CurrentUser.OpenSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {

                foreach (XElement xElement in mappings.Elements("mapping"))
                {
                    string fileContent = File.ReadAllText($"{fileDirPath}/{xElement.Value}");
                    byte[] bytes = new UTF8Encoding().GetBytes(fileContent);
                    //Add null byte for registry
                    bytes = bytes.Append((byte)0x00).ToArray();

                    keyToWrite.SetValue(xElement.Attribute("regValueName").Value, bytes, RegistryValueKind.Binary);
                }
            }
        }

        /// <summary>
        /// Dumps the the values from a HKEY_CURRENT_USER registry key into files and creates a "RegMappings.xml" file to keep track of the files.
        /// </summary>
        /// <param name="keyName">Registry to dump the values from.</param>
        /// <param name="dir">Directory in which the files will be created.</param>
        public static void DumpHKCUBinaryValuesToFiles(string keyName, string dir)
        {
            string fileDirPath = Path.GetFullPath(dir);

            //If dir does not exist create it.
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            //Else clean it.
            else
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }

            Dictionary<string, string> registryExportMappings = new Dictionary<string, string>();

            using (RegistryKey keyToDump = Registry.CurrentUser.OpenSubKey(keyName, RegistryRights.ReadKey))
            {

                foreach (var valueName in keyToDump.GetValueNames())
                {

                    object value = keyToDump.GetValue(valueName);


                    string outStr = "";

                    if (value.GetType() == typeof(byte[]))
                    {
                        byte[] valueAsByteArr = (byte[])value;
                        //Remove null byte at the end.
                        valueAsByteArr = valueAsByteArr.Take(valueAsByteArr.Length - 1).ToArray();
                        outStr = System.Text.Encoding.Default.GetString(valueAsByteArr);
                    }
                    else
                    {
                        outStr = value.ToString();
                        continue;
                    }

                    string validNameFromRegValueName = RemoveAllChars(valueName, Path.GetInvalidFileNameChars());

                    string fileName = $"{validNameFromRegValueName}.xml";
                    File.WriteAllText($"{fileDirPath}\\{fileName}", outStr);

                    registryExportMappings.Add(valueName, fileName);

                }
            }

            //Create mappings file.
            XElement mappings = new XElement("mappings");
            foreach (var kvp in registryExportMappings)
            {
                mappings.Add(new XElement("mapping", new XAttribute("regValueName", kvp.Key), kvp.Value));
            }
            mappings.Save($"{fileDirPath}/RegMappings.xml");
        }

        /// <summary>
        /// Method to remove specific chars from a string.
        /// </summary>
        /// <param name="str">String to remove chars from.</param>
        /// <param name="chars">Chars to remove.</param>
        /// <returns></returns>
        private static string RemoveAllChars(string str, char[] chars)
        {
            foreach (char c in chars)
            {
                str = str.Replace(c, ' ');
            }

            return str;
        }
    }
}
