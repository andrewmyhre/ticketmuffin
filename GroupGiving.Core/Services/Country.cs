namespace GroupGiving.Core.Services
{
    public class Country
    {
        public Country(string name)
        {
            this.Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}