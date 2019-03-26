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

namespace APITool.Print
{
    public class JsonMemberWriter : DefaultMemberWriter
    {
        AssemblyDocument _asmDoc;
        bool _isFirstMember;

        public override void EmitDocumentBegin()
        {
            _isFirstMember = true;
            Writer.WriteLine("[");
            Writer.Flush();
        }

        public override void EmitDocumentEnd()
        {
            Writer.WriteLine();
            Writer.WriteLine("]");
            Writer.Flush();
        }

        public override void EmitAssemblyBegin(AssemblyDefinition def)
        {
            _asmDoc = new AssemblyDocument(def);
        }

        public override void WriteLine(IMemberDefinition member, bool isHidden)
        {
            string docId = DocCommentId.GetDocCommentId(member);
            string baseType = string.Empty;
            string declType = string.Empty;
            string declNamespace = string.Empty;
            string retType = string.Empty;
            string constValue = string.Empty;
            bool isStatic = false;

            var typeDef = member as TypeDefinition;
            if (typeDef != null)
            {
                baseType = typeDef.BaseType?.FullName;
                declNamespace = typeDef.Namespace;
            }

            if (member.DeclaringType != null)
            {
                declType = member.DeclaringType.FullName;
                declNamespace = member.DeclaringType.Namespace;
            }

            var methodDef = member as MethodDefinition;
            if (methodDef != null)
            {
                retType = methodDef.ReturnType.FullName;
                isStatic = methodDef.IsStatic;
            }

            var eventDef = member as EventDefinition;
            if (eventDef != null)
            {
                retType = eventDef.EventType.FullName;
            }

            var propDef = member as PropertyDefinition;
            if (propDef != null)
            {
                retType = propDef.PropertyType.FullName;
            }

            var fieldDef = member as FieldDefinition;
            if (fieldDef != null)
            {
                retType = fieldDef.FieldType.FullName;
                constValue = fieldDef.Constant?.ToString();
                isStatic = fieldDef.IsStatic;
            }

            string sinceTizen = string.Empty;
            List<string> privileges = new List<string>();
            List<string> features = new List<string>();

            XmlNode xmlNode = _asmDoc.GetMemberNode(docId);
            if (xmlNode != null)
            {
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
                privileges.Sort();
                features.Sort();
            }

            if (!_isFirstMember)
            {
                Writer.WriteLine(",");
            }
            _isFirstMember = false;

            Writer.WriteLine("  {");
            Writer.WriteLine("    \"DocId\": \"{0}\",", docId);
            Writer.WriteLine("    \"Info\": {");
            Writer.WriteLine("      \"Signature\": \"{0}\",", member.FullName);
            if (!string.IsNullOrEmpty(baseType))
                Writer.WriteLine("      \"BaseType\": \"{0}\",", baseType);
            if (!string.IsNullOrEmpty(declType))
                Writer.WriteLine("      \"DeclaringType\": \"{0}\",", declType);
            if (!string.IsNullOrEmpty(declNamespace))
                Writer.WriteLine("      \"Namespace\": \"{0}\",", declNamespace);
            if (!string.IsNullOrEmpty(retType))
                Writer.WriteLine("      \"ReturnType\": \"{0}\",", retType);
            if (!string.IsNullOrEmpty(constValue))
                Writer.WriteLine("      \"Constant\": \"{0}\",", constValue);
            Writer.WriteLine("      \"IsStatic\": {0},", isStatic ? "true" : "false");
            Writer.WriteLine("      \"IsHidden\": {0},", isHidden ? "true" : "false");
            Writer.WriteLine("      \"IsObsolete\": {0},", IsObsoleteMember(member) ? "true" : "false");
            if (string.IsNullOrEmpty(sinceTizen)) {
                sinceTizen = "none";
            }
            Writer.WriteLine("      \"Since\": \"{0}\"{1}", sinceTizen, (privileges.Count + features.Count > 0) ? "," : string.Empty);
            if (privileges.Count > 0)
            {
                Writer.WriteLine("      \"Privileges\": [");
                for (var i = 0; i < privileges.Count; i++)
                {
                    Writer.WriteLine("        \"{0}\"{1}", privileges[i], i < privileges.Count - 1 ? "," : string.Empty);
                }
                Writer.WriteLine("      ]{0}", features.Count > 0 ? "," : string.Empty);
            }
            if (features.Count > 0)
            {
                Writer.WriteLine("      \"Features\": [");
                for (var i = 0; i < features.Count; i++)
                {
                    Writer.WriteLine("        \"{0}\"{1}", features[i], i < features.Count - 1 ? "," : string.Empty);
                }
                Writer.WriteLine("      ]");
            }
            Writer.WriteLine("    }");
            Writer.Write("  }");

            Writer.Flush();

        }
    }
}