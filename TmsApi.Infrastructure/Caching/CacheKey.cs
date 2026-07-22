namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    private const string SchemaVersion = "v2";

    public static string Course(int id) =>
        $"{SchemaVersion}:course:{id}";

    public static string CoursesPage(int page, int pageSize, string? search, string? orderBy, bool descending) =>
        $"{SchemaVersion}:courses:page={page}:size={pageSize}:search={search ?? ""}:order={orderBy ?? ""}:desc={descending}";

    public const string CoursesTag = "courses";
}