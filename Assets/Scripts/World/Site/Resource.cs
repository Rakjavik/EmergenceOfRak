namespace rak.world
{
    public enum ResourceType { NONE, Matter, Energy }
    public interface Resource
    {
        ResourceType GetResourceType();
    }
}
