namespace ASMC.Common
{
    public interface ISupportDialog
    {
        bool? DialogResult
        {
            get;
        }

        void Initialize();
    }
}
