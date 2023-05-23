namespace AttendanceSystem.Settings
{
    public class DatabaseSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? AttendanceCollectionName { get; set; }
        public string? UserCollectionName { get; set; }
        public string? OutletCollectionName { get; set; }
        public string? OutletVisitCollectionName { get; set; }
    }
}
