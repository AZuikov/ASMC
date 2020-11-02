using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices;
using ASMC.Devices.WithoutInterface.Voltmetr;

namespace B5_71_PRO_Abstract
{
    public class SelectRangeViewModel : SelectionViewModel 

    {
    private string _description = "Выбор установленного предела измерения:";
    private List<MeasPoint<Voltage>> _ranges;
    private MeasPoint<Voltage> _selectRange;

    /// <summary>
    /// Текст комментария для пользователя.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value, nameof(Description));
    }

    /// <summary>
    /// Списко пределов, в выпадающем списке.
    /// </summary>
    public List<MeasPoint<Voltage>> Ranges
    {
        get => _ranges;
        set => SetProperty(ref _ranges, value, nameof(Ranges));
    }

    /// <summary>
    /// Выбранный пользователем предел.
    /// </summary>
    public MeasPoint<Voltage> SelectRange
    {
        get => _selectRange;
        set => SetProperty(ref _selectRange, value, nameof(SelectRange));
    }

    public SelectRangeViewModel()
    {
        var b357 = new B3_57(); 
            _ranges = new List<MeasPoint<Voltage>>();
            foreach (var val in b357.Ranges)
        {
            _ranges.Add(val);
        }
        SelectRange = Ranges.FirstOrDefault();

    }
    }




}
