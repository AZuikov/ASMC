namespace ASMC.Core
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
