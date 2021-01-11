using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;

namespace ASMC.Devices.Interface.Multimetr.Mode
{
    public interface IMeasureMode<T>:IDevice
    {
        #region Property

        bool IsEnable { get;  }
        T Range { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Получить измеренное значение физической величины с прибора.
        /// </summary>
        /// <returns>Значение измеренной физ. величины.</returns>
        T Value { get; }



        #endregion
    }

    public interface IVoltageGroup: IDcVoltage, IAcVoltage
    {
    
    }


    public interface IResolution<T>
    {
        void SetResolution(T inResolution);
    }
    public interface IDcVoltage
    {
        #region Property

        IMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }

        #endregion
    }

    public interface IAcVoltage
    {
        #region Property

        IMeasureMode<MeasPoint<Voltage>> AcVoltage { get; set; }

        #endregion
    }

    public interface IAcFilter
    {
        void SetFilter(MeasPoint<Frequency> filterFreq);
    }

    public interface ICurrentGroup: IDcCurrent,IAcCurrent
    {
        
    }

    

    public interface IDcCurrent
    {
        #region Property

        IMeasureMode<MeasPoint<Current>> DcCurrent { get; set; }

        #endregion
    }

    public interface IAcCurrent
    {
        #region Property

        IMeasureMode<MeasPoint<Current>> AcCurrent { get; set; }

        #endregion
    }

    

    public interface IResistanceGroup: IResistance2W, IResistance4W
    {
       
    }

    public interface IResistance2W
    {
        #region Property

        IMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }

        #endregion
    }

    public interface IResistance4W
    {
        #region Property

        IMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }

        #endregion
    }

    public interface ICapacity
    {
        #region Property

        IMeasureMode<MeasPoint<Capacity>> Capacity { get; set; }

        #endregion
    }

    public interface IFrequency
    {
        #region Property

        IMeasureMode<MeasPoint<Frequency>> Frequency { get; set; }

        #endregion
    }

    public interface ITime
    {
        #region Property

        IMeasureMode<MeasPoint<Time>> Time { get; set; }

        #endregion
    }

    public interface ITemperature
    {
        #region Property

        IMeasureMode<MeasPoint<Temperature>> Temperature { get; set; }

        #endregion
    }
}

namespace ASMC.Devices.Interface.Multimetr
{
    public interface IMultimetr : IResistanceGroup, IVoltageGroup, ICurrentGroup, IUserType, IDevice
    {
    }
}