using System;
using System.IO;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public interface IFileLocator
    {
        FileInfo GetFileInfo(int id);
    }
}
