namespace SilkyNvg.Blending
{
    /// <summary>
    /// <para>The composite operations in NanoVG are modeled after HTML Canvas API, and
    /// the blend func is based on OpenGL (see corresponding manuals for more info).
    /// The colours in the blending state have premultiplied alpha.</para>
    /// </summary>
    public static class NvgBlending
    {

        /// <summary>
        /// Sets the composite operation.
        /// </summary>
        public static void GlobalCompositeOperation(this Nvg nvg, CompositeOperation op)
        {
            nvg.stateStack.CurrentState.CompositeOperation = new CompositeOperationState(op);
        }

        /// <summary>
        /// Sets the composite operation with custom pixel arithmetic.
        /// </summary>
        public static void GlobalCompositeBlendFunc(this Nvg nvg, BlendFactor sfactor, BlendFactor dfactor)
        {
            GlobalCompositeBlendFuncSeperate(nvg, sfactor, sfactor, dfactor, dfactor);
        }

        /// <summary>
        /// Sets the composite operationi with custom pixel arithmetic for RGB and alpha components seperately.
        /// </summary>
        public static void GlobalCompositeBlendFuncSeperate(this Nvg nvg, BlendFactor srcRgb, BlendFactor dstRgb, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            nvg.stateStack.CurrentState.CompositeOperation = new CompositeOperationState(srcRgb, dstRgb, srcAlpha, dstAlpha);
        }

    }
}
