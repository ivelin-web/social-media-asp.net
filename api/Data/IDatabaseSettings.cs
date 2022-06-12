namespace api.Data
{
    public interface IDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string PostCollectionName { get; set; }

        public string UserCollectionName { get; set; }
    }
}
