namespace Celeste.Entities;
internal class SparkyDust
{
    // 3D Tower Dust Bunny Node - represents a node in a dust tower structure

    public Vector3 Position { get; set; }
    public float Size { get; set; }
    public float Node { get; set; }
    public int TowerLevel { get; set; }
    public float DustDensity { get; set; }
    public float SpinSpeed { get; set; }
    public float CurrentRotation { get; set; }
    public List<SparkyDust> ConnectedNodes { get; private set; }
    public bool IsActive { get; set; }

    public SparkyDust(Vector3 position, float size, float node, int towerLevel = 0)
    {
        Position = position;
        Size = size;
        Node = node;
        TowerLevel = towerLevel;
        DustDensity = 0.5f + (towerLevel * 0.1f); // Denser dust at higher levels
        SpinSpeed = 1.0f - (towerLevel * 0.1f); // Slower spin at higher levels
        CurrentRotation = 0f;
        ConnectedNodes = new List<SparkyDust>();
        IsActive = true;
    }

    public void ConnectToNode(SparkyDust otherNode)
    {
        if (otherNode != null && !ConnectedNodes.Contains(otherNode))
        {
            ConnectedNodes.Add(otherNode);
            otherNode.ConnectedNodes.Add(this);
        }
    }

    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        // Rotate the dust bunny node
        CurrentRotation += SpinSpeed * deltaTime;
        if (CurrentRotation >= 360f)
            CurrentRotation -= 360f;

        // Pulsate dust density based on connections
        float connectionInfluence = ConnectedNodes.Count * 0.05f;
        DustDensity = Math.Min(1.0f, 0.5f + (TowerLevel * 0.1f) + connectionInfluence);

        // Update connected nodes influence
        updateTowerDynamics(deltaTime);
    }

    private void updateTowerDynamics(float deltaTime)
    {
        // Tower nodes affect each other's dust patterns
        foreach (var connectedNode in ConnectedNodes)
        {
            float distance = Vector3.Distance(Position, connectedNode.Position);
            float influence = 1.0f / (1.0f + distance * 0.1f);

            // Synchronize some rotation for visual cohesion
            float rotationDiff = CurrentRotation - connectedNode.CurrentRotation;
            if (Math.Abs(rotationDiff) > 180f)
                rotationDiff = rotationDiff > 0 ? rotationDiff - 360f : rotationDiff + 360f;

            CurrentRotation += rotationDiff * influence * 0.1f * deltaTime;
        }
    }

    public Vector3 GetDustParticlePosition(int particleIndex, int totalParticles)
    {
        // Generate dust particle positions around the node in a 3D pattern
        float angle = (particleIndex / (float)totalParticles) * 360f + CurrentRotation;
        float radius = Size * DustDensity;
        float heightOffset = (float)Math.Sin(angle * Math.PI / 180f) * Size * 0.3f;

        float x = Position.X + (float)Math.Cos(angle * Math.PI / 180f) * radius;
        float y = Position.Y + heightOffset;
        float z = Position.Z + (float)Math.Sin(angle * Math.PI / 180f) * radius * 0.5f;

        return new Vector3(x, y, z);
    }

    public void Deactivate()
    {
        IsActive = false;
        // Disconnect from all nodes
        foreach (var node in ConnectedNodes.ToList())
        {
            node.ConnectedNodes.Remove(this);
        }
        ConnectedNodes.Clear();
    }
}



