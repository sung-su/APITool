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
using System.Linq;
using Mono.Cecil;

namespace APITool
{
    public class APIPrinter : AssemblyProcessor, IDisposable
    {
        readonly PrintOptions _options;
        readonly bool _isPrintAll;
        readonly StreamWriter _writer;
        readonly IMemberFormatter _formatter;

        public APIPrinter(PrintOptions options)
        {
            _options = options;
            _isPrintAll = !(options.PrintTypes | options.PrintFields | options.PrintProperties | options.PrintEvents | options.PrintMethods);

            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                _writer = new StreamWriter(options.OutputFile);
            }
            else
            {
                _writer = new StreamWriter(Console.OpenStandardOutput());
            }

            if (_options.OutputFormat.Equals("CSV"))
            {
                _formatter = new CSVFormatter();
            }
            else
            {
                _formatter = new XmlDocIdsFormatter();
            }
        }

        public void Run(string filepath)
        {
            _formatter.Prepare(filepath);

            var asm = AssemblyDefinition.ReadAssembly(filepath);
            ProcessAssembly(asm);
        }

        protected override void ProcessType(TypeDefinition typeDef)
        {
            // Only print public and nested public types.
            if (!typeDef.IsPublic && !typeDef.IsNestedPublic)
            {
                return;
            }

            bool isHidden = IsHidden(typeDef);
            if (_options.PrintHiddens || !isHidden)
            {
                if (_isPrintAll || _options.PrintTypes)
                {
                    _writer.WriteLine(_formatter.Format(typeDef, isHidden));
                    _writer.Flush();
                }
                base.ProcessType(typeDef);
            }
        }

        protected override void ProcessField(FieldDefinition fieldDef)
        {
            // Only print public and protected fields.
            if (!fieldDef.IsPublic && !fieldDef.IsFamily)
            {
                return;
            }

            // Don't print specialname
            if (fieldDef.IsSpecialName)
            {
                return;
            }

            bool isHidden = IsHidden(fieldDef);
            if ((_isPrintAll || _options.PrintFields) && (_options.PrintHiddens || !isHidden))
            {
                _writer.WriteLine(_formatter.Format(fieldDef, isHidden));
                _writer.Flush();
            }
        }

        protected override void ProcessProperty(PropertyDefinition propDef)
        {
            bool isPublicOrFamaily = false;
            if (propDef.GetMethod != null)
            {
                isPublicOrFamaily = propDef.GetMethod.IsPublic || propDef.GetMethod.IsFamily;
            }
            if (propDef.SetMethod != null)
            {
                isPublicOrFamaily |= propDef.SetMethod.IsPublic || propDef.SetMethod.IsFamily;
            }

            // Only print public and protected members.
            if (!isPublicOrFamaily)
            {
                return;
            }

            bool isHidden = IsHidden(propDef);
            if ((_isPrintAll || _options.PrintProperties) && (_options.PrintHiddens || !isHidden))
            {
                _writer.WriteLine(_formatter.Format(propDef, isHidden));
                _writer.Flush();
            }
        }

        protected override void ProcessEvent(EventDefinition eventDef)
        {
            bool isPublicOrFamaily = false;
            if (eventDef.AddMethod != null)
            {
                isPublicOrFamaily = eventDef.AddMethod.IsPublic || eventDef.AddMethod.IsFamily;
            }
            if (eventDef.RemoveMethod != null)
            {
                isPublicOrFamaily |= eventDef.RemoveMethod.IsPublic || eventDef.RemoveMethod.IsFamily;
            }

            // Only print public and protected members.
            if (!isPublicOrFamaily)
            {
                return;
            }

            bool isHidden = IsHidden(eventDef);
            if ((_isPrintAll || _options.PrintEvents) && (_options.PrintHiddens || !isHidden))
            {
                _writer.WriteLine(_formatter.Format(eventDef, isHidden));
                _writer.Flush();
            }
        }

        protected override void ProcessMethod(MethodDefinition methodDef)
        {
            bool isExplicitImpl = false;
            foreach (var ot in methodDef.Overrides)
            {
                if (methodDef.Name.StartsWith(ot.DeclaringType.FullName))
                {
                    isExplicitImpl = true;
                    break;
                }
            }

            // Only print public / protected and explicit interface methods.
            if (!methodDef.IsPublic && !methodDef.IsFamily && !isExplicitImpl)
            {
                return;
            }

            // Don't print event's add/remove and property's getter/setter methods.
            if (methodDef.IsAddOn || methodDef.IsRemoveOn || methodDef.IsGetter || methodDef.IsSetter)
            {
                return;
            }

            bool isHidden = IsHidden(methodDef);
            if ((_isPrintAll || _options.PrintMethods) && (_options.PrintHiddens || !isHidden))
            {
                _writer.WriteLine(_formatter.Format(methodDef, isHidden));
                _writer.Flush();
            }
        }

        bool IsHidden(IMemberDefinition member)
        {
            if (IsHiddenInCustomAttributes(member.CustomAttributes))
            {
                return true;
            }

            var declType = member.DeclaringType;
            if (declType != null)
            {
                return IsHidden(declType);
            }
            return false;
        }

        bool IsHiddenInCustomAttributes(Mono.Collections.Generic.Collection<CustomAttribute> attrs)
        {
            bool ret = false;
            var attr = attrs.FirstOrDefault(a => a.AttributeType.FullName == "System.ComponentModel.EditorBrowsableAttribute");
            if (attr != null)
            {
                if (attr.ConstructorArguments.Count > 0)
                {
                    ret = (int)attr.ConstructorArguments[0].Value == (int)System.ComponentModel.EditorBrowsableState.Never;
                }
            }
            return ret;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
                }

                disposedValue = true;
            }
        }

        ~APIPrinter() {
           Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
