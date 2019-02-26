using System.IO;
using Mono.Cecil;

namespace APITool.Print
{
    public interface IMemberWriter
    {
        void SetStreamWriter(StreamWriter writer);

        void WriteLine(IMemberDefinition member);

        void WriteLine(IMemberDefinition member, bool isHidden);

        void EmitDocumentBegin();

        void EmitDocumentEnd();

        void EmitAssemblyBegin(AssemblyDefinition def);

        void EmitAssemblyEnd(AssemblyDefinition def);

    }

}