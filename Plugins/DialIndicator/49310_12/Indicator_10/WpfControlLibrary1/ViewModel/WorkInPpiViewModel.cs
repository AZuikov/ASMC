using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;

namespace Indicator_10.ViewModel
{
    public class WorkInPpiViewModel: SelectionViewModel
    {
        public WebCamViewModel WebCam { get; private set; } 

        public TableViewModel Content { get; set; }
        /// <inheritdoc />
        protected override void OnInitializing()
        {
            base.OnInitializing();
            WebCam = new WebCamViewModel();
            //Content.

        }
    }
}
