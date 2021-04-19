namespace SilkyNvg
{

    /// <summary>
    /// Wheather to leave a hole or not.
    /// Uses <see cref="Winding"/> to specify.
    /// </summary>
    public enum Solidity
    {

        /// <summary>
        /// Not a hole, <see cref="Winding.CCW"/>
        /// </summary>
        Solid = Winding.CCW,
        /// <summary>
        /// Leave space or hole, <see cref="Winding.CW"/>
        /// </summary>
        Hole = Winding.CW

    }
}
