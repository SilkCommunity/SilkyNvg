namespace SilkyNvg.Utils
{
    // Necessary floating point precision as well as
    // tesselation tolerance are dependent on pixel ratio.
    // This class deals with calculating the relevant values depending on the current pixel ratio.
    internal class RenderTolerances
    {

        internal float FloatCompareTol { get; private set; } = 0.01f;

        internal float MaxPathLength => 32.0f;
        
        internal void UpdateTols(float pixelRatio)
        {
            FloatCompareTol = 0.01f / pixelRatio;
        }
    
    }  
}
