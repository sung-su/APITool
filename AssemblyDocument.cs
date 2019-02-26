using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using Mono.Cecil;

namespace APITool
{
    public class AssemblyDocument
    {
        Dictionary<string, XmlNode> _xmlNodes = new Dictionary<string, XmlNode>();

        public AssemblyDocument(AssemblyDefinition asm)
        {
            string xmlpath = Path.ChangeExtension(asm.MainModule.FileName, "xml");
            if (File.Exists(xmlpath))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlpath);

                foreach (XmlNode docNode in xmlDoc.DocumentElement.ChildNodes)
                {
                    foreach (XmlNode memberNode in docNode)
                    {
                        if (memberNode.Name == "member")
                        {
                            _xmlNodes[memberNode.Attributes["name"].Value] = memberNode;
                        }
                    }
                }
            }
        }

        public XmlNode GetMemberNode(string docId) {
            XmlNode xmlNode = null;
            _xmlNodes.TryGetValue(docId, out xmlNode);
            return xmlNode;
        }
    }

}