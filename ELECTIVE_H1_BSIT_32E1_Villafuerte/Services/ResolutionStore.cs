using ResolutionsApi.Models;

namespace ResolutionsApi.Services;

public static class ResolutionStore
{
    public static List<Resolution> Items { get; } = new();
    public static int NextId = 1;
}
