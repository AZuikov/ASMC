using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;

namespace Indicator_10.ViewModel
{
    public class WorkInPpiViewModel: FromBaseViewModel
    {
        protected WebCamViewModel WebCam { get; private set; } 

        protected TableViewModel Content { get; private set; }
        /// <inheritdoc />
        protected override void OnInitializing()
        {
            base.OnInitializing();
            WebCam = new WebCamViewModel();
            //Content.

        }
    }
}
