namespace SilkyNvg.Blending
{
    public static class NvgBlending
    {

        public static void GlobalCompositeOperation(this Nvg nvg, CompositeOperation op)
        {
            nvg.stateStack.CurrentState.CompositeOperation = new CompositeOperationState(op);
        }

        public static void GlobalCompositeBlendFunc(this Nvg nvg, BlendFactor sfactor, BlendFactor dfactor)
        {
            GlobalCompositeBlendFuncSeperate(nvg, sfactor, sfactor, dfactor, dfactor);
        }

        public static void GlobalCompositeBlendFuncSeperate(this Nvg nvg, BlendFactor srcRgb, BlendFactor dstRgb, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            nvg.stateStack.CurrentState.CompositeOperation = new CompositeOperationState(srcRgb, dstRgb, srcAlpha, dstAlpha);
        }

    }
}
