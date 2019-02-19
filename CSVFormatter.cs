/*
 * Copyright (c) 2019 Samsung Electronics Co., Ltd All Rights Reserved
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace APITool
{
    // CSV format
    // Type, DocId, ReturnType or BaseType, Const Value, IsStatic, IsHidden, SinceTizen, Privileges, Features
    public class CSVFormatter : IMemberFormatter
    {
        Dictionary<string, XmlNode> _xmlNodes = new Dictionary<string, XmlNode>();

        public void Prepare(string filepath)
        {
            string xmlpath = Path.ChangeExtension(filepath, "xml");
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

        public string Format(IMemberDefinition member, bool isHidden)
        {
            string xmlDocId = DocCommentId.GetDocCommentId(member);
            string refType = string.Empty;
            string constValue = string.Empty;
            string strPrivileges = string.Empty;
            string strFeatures = string.Empty;
            string sinceTizen = string.Empty;
            bool isStatic = false;

            XmlNode xmlNode = null;
            if (_xmlNodes.TryGetValue(xmlDocId, out xmlNode))
            {
                List<string> privileges = new List<string>();
                List<string> features = new List<string>();

                foreach (XmlNode childNode in xmlNode)
                {
                    if (childNode.Name == "privilege")
                    {
                        privileges.AddRange(Regex.Split(childNode.InnerText.Trim(), @"\s+"));
                    }
                    else if (childNode.Name == "feature")
                    {
                        features.AddRange(Regex.Split(childNode.InnerText.Trim(), @"\s+"));
                    }
                    else if (childNode.Name == "since_tizen")
                    {
                        sinceTizen = childNode.InnerText.Trim();
                    }
                }

                if (privileges.Count > 0)
                {
                    privileges.Sort();
                    strPrivileges = string.Join(' ', privileges.ToArray());
                }

                if (features.Count > 0)
                {
                    features.Sort();
                    strFeatures = string.Join(' ', features.ToArray());
                }
            }

            var typeDef = member as TypeDefinition;
            if (typeDef != null)
            {
                refType = typeDef.BaseType?.FullName;
            }

            var methodDef = member as MethodDefinition;
            if (methodDef != null)
            {
                refType = methodDef.ReturnType.FullName;
                isStatic = methodDef.IsStatic;
            }

            var eventDef = member as EventDefinition;
            if (eventDef != null)
            {
                refType = eventDef.EventType.FullName;
            }

            var propDef = member as PropertyDefinition;
            if (propDef != null)
            {
                refType = propDef.PropertyType.FullName;
            }

            var fieldDef = member as FieldDefinition;
            if (fieldDef != null)
            {
                refType = fieldDef.FieldType.FullName;
                constValue = fieldDef.Constant?.ToString();
                isStatic = fieldDef.IsStatic;
            }

            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    xmlDocId[0], xmlDocId,
                    refType, constValue,
                    isStatic ? "static" : string.Empty,
                    isHidden ? "hidden" : string.Empty,
                    sinceTizen, strPrivileges, strFeatures);

        }

    }
}
