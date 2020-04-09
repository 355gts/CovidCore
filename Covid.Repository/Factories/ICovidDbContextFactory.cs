namespace Covid.Repository.Factories
{
    public interface ICovidDbContextFactory
    {
        ICovidDbContext GetContext();
    }
}
