using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    public interface IInstruction
    {

        bool RequiresPosition { get; }

        float ID { get; }

        float[] FieldsAsFloatArray { get; }

        void Prepare();
        void FlattenPath(PathCache cache, Style style);

        void Execute();

    }
}
