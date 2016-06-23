using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.Chisanu.Plugin.Touch
{

    /// <summary>
    /// Đã biết được nhiều thứ hay ho từ code này
    /// Dùng một đối tượng XDocument như một kho quản lí
    /// Load cho lần đầu
    /// Cứ cập nhật runtime
    /// khi nào thoát thì sẽ tự save
    /// </summary>
    public static class Settings
    {
        // Fields
        private static XDocument _xml;
        private static string _xmlFile = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Config\TouchCommunication.xml");

        // Methods
        public static void Load()
        {
            try
            {
                _xml = XDocument.Load(_xmlFile);
            }
            catch
            {
                _xml = new XDocument(new XDeclaration("1.0", "utf-8", null), new object[] { new XElement("TouchCommunication", new object[] { new XElement("Keys", new XElement("Key", new object[] { new XAttribute("Function", "PluginToggle"), new XAttribute("Code", "f11") })), new XElement("PluginEnabled", "true") }) });
            }
        }

        public static bool PluginEnabledToggle()
        {
            PluginEnabled = !PluginEnabled;
            return PluginEnabled;
        }

        public static void Save()
        {
            try
            {
                _xml.Save(_xmlFile);
            }
            catch
            {
            }
        }

        // Properties
        public static string KeyPluginToggle
        {
            get
            {
                return (from e in _xml.Descendants("Key")
                        where e.Attribute("Function").Value == "PluginToggle"
                        select e.Attribute("Code").Value).FirstOrDefault<string>();
            }
        }

        public static bool PluginEnabled
        {
            get
            {
                return (_xml.Descendants("PluginEnabled").FirstOrDefault<XElement>().Value == "true");
            }
            set
            {
                _xml.Descendants("PluginEnabled").FirstOrDefault<XElement>().Value = value ? "true" : "false";
            }
        }
    }
}