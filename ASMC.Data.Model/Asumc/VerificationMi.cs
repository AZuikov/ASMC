namespace ASMC.Data.Model.Asumc
{
    public class VerificationMi
    {
        public VerificationSi Ekz { get; set; }
        public string Result { get; set; }
        public bool IsGood { get; set; } = true;
    }
}
