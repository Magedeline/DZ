namespace Celeste.Entities
{
    /// <summary>
    /// Simple wire entity for cutscene effects
    /// </summary>
    public class Wire : Entity
    {
        public SimpleCurve Curve { get; set; }
        
        public Wire(Vector2 start, Vector2 end) : base(start)
        {
            Curve = new SimpleCurve(start, end, Vector2.Zero);
        }
    }
}



