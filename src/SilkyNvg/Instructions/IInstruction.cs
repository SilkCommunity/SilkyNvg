using SilkyNvg.Core;
using SilkyNvg.Paths;

namespace SilkyNvg.Instructions
{
    internal interface IInstruction
    {

        bool RequiresPosition { get; }

        float ID { get; }

        float[] FieldsAsFloatArray { get; }

        void Prepare();
        void FlattenPath(PathCache cache, Style style);

        void Execute();

    }
}
