namespace SilkyNvg
{

    /// <summary>
    /// Combinations of 2 <see cref="BlendFactor"/>s. They are commonly
    /// used together and used to set the render style more easilly.
    /// </summary>
    public enum CompositeOperation
    {
        /// <summary>
        /// <see cref="BlendFactor.One"/> and <see cref="BlendFactor.OneMinusSrcAlpha"/>
        /// </summary>
        SourceOver,
        /// <summary>
        /// <see cref="BlendFactor.DstAlpha"/> and <see cref="BlendFactor.Zero"/>
        /// </summary>
        SourceIn,
        /// <summary>
        /// <see cref="BlendFactor.OneMinusDstAlpha"/> and <see cref="BlendFactor.Zero"/>
        /// </summary>
        SourceOut,
        /// <summary>
        /// <see cref="BlendFactor.DstAlpha"/> and <see cref="BlendFactor.OneMinusSrcAlpha"/>
        /// </summary>
        Atop,
        /// <summary>
        /// <see cref="BlendFactor.OneMinusDstAlpha"/> and <see cref="BlendFactor.One"/>
        /// </summary>
        DestinationOver,
        /// <summary>
        /// <see cref="BlendFactor.Zero"/> and <see cref="BlendFactor.SrcAlpha"/>
        /// </summary>
        DestinationIn,
        /// <summary>
        /// <see cref="BlendFactor.Zero"/> and <see cref="BlendFactor.OneMinusSrcAlpha"/>
        /// </summary>
        DestinationOut,
        /// <summary>
        /// <see cref="BlendFactor.OneMinusDstAlpha"/> and <see cref="BlendFactor.SrcAlpha"/>
        /// </summary>
        DestinationAtop,
        /// <summary>
        /// <see cref="BlendFactor.One"/> and <see cref="BlendFactor.One"/>
        /// </summary>
        Lighter,
        /// <summary>
        /// <see cref="BlendFactor.One"/> and <see cref="BlendFactor.Zero"/>
        /// </summary>
        Copy,
        /// <summary>
        /// <see cref="BlendFactor.OneMinusDstAlpha"/> and <see cref="BlendFactor.OneMinusSrcAlpha"/>
        /// </summary>
        XOr

    }
}