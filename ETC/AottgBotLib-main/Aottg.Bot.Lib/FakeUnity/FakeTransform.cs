namespace FakeUnity
{
    public sealed class FakeTransform : FakeComponent
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }
    }
}