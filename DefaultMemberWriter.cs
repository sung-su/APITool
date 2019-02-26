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
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace APITool
{
    public class DefaultMemberWriter : IMemberWriter, IDisposable
    {
        protected StreamWriter Writer { get; set; }

        public DefaultMemberWriter()
        {
            Writer = new StreamWriter(Console.OpenStandardOutput());
        }

        public void SetStreamWriter(StreamWriter writer)
        {
            Writer = writer;
        }

        public void WriteLine(IMemberDefinition member)
        {
            WriteLine(member, false);
        }

        public virtual void WriteLine(IMemberDefinition member, bool isHidden)
        {
            Writer.WriteLine(DocCommentId.GetDocCommentId(member));
            Writer.Flush();
        }

        public virtual void EmitDocumentBegin()
        {

        }

        public virtual void EmitDocumentEnd()
        {

        }

        public virtual void EmitAssemblyBegin(AssemblyDefinition def)
        {

        }

        public virtual void EmitAssemblyEnd(AssemblyDefinition def)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Writer.Dispose();
                }

                disposedValue = true;
            }
        }

        ~DefaultMemberWriter()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
