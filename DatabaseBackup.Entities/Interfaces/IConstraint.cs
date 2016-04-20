namespace DatabaseBackup.Entities.Interfaces
{
    public interface IConstraint
    {
        string Name { get; set; }
        Table Table { get; set; }
    }
}