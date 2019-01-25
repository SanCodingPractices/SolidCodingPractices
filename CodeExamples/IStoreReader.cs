using System;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public interface IStoreReader
    {
        Maybe<string> Read(int id);
    }
}
