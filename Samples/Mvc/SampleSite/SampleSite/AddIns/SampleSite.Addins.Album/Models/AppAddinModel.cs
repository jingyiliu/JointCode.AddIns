namespace SampleSite.Addins.Album.Models
{
    public class AppAddinModel
    {
        //public AppAddinModel()
        //{
        //    Children = new List<AppAddinModel>();
        //}

        public string Id { get; set; }
        //public string ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //public List<AppAddinModel> Children { get; set; }

        public AppAssembly AssemblyInfo {get;set;}

        //public IMvcPlugin Plugin { get; set; }
    }
}
