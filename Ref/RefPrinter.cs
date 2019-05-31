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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace APITool.Ref
{
    public class RefPrinter
    {
        readonly RefOptions _options;

        public RefPrinter(RefOptions options)
        {
            _options = options;
        }

        public void PrintReferences(string targetFile)
        {
            if (_options.Verbose)
            {
                Log.Verbose($"Target File: {targetFile}");
            }

            var asm = AssemblyDefinition.ReadAssembly(targetFile);

            if (_options.Verbose)
            {
                Log.Verbose($"Target Assembly: {asm.FullName}");
            }

            var references = new List<AssemblyNameReference>();
            CollectReferences(asm, references);

            foreach (var asmRef in references.OrderBy(r => r.Name).Distinct())
            {
                if (_options.NameOnly)
                {
                    Console.WriteLine(asmRef.Name);
                }
                else
                {
                    Console.WriteLine(asmRef.FullName);
                }
            }
        }

        void CollectReferences(AssemblyDefinition asmDef, IList<AssemblyNameReference> references)
        {
            foreach (var module in asmDef.Modules)
            {
                foreach (var asmRef in module.AssemblyReferences)
                {
                    references.Add(asmRef);
                }
            }
        }
    }

}