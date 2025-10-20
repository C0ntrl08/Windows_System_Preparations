namespace LanguagePacksCheckerAndSetter.Models
{
    public class LanguagePackModel
    {
        private string _packageIdentity = string.Empty;
        public string PackageIdentity
        {
            get { return _packageIdentity; }
            set { _packageIdentity = value; }
        }

        //public override string ToString()
        //{
        //    return PackageIdentity;
        //}
    }
}
