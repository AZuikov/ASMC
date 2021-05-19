namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс содержащий типовые настройки входа частотомера.
    /// </summary>
    public interface ITypicalCounterInputSettings : ICounterInputSlopeSetting
    {
        /// <summary>
        /// Установить аттенюатор 1:1.
        /// </summary>
        public void SetAtt_1();

        /// <summary>
        /// Установить аттенюатор 1:10.
        /// </summary>
        public void SetAtt_10();

        public string GetAtt();

        /// <summary>
        ///Установить максимальный входной импеданс.
        /// </summary>
        public void SetHightImpedance();

        /// <summary>
        /// Установить минимальный входной импеданс.
        /// </summary>
        public void SetLowImpedance();

        public string GetImpedance();

        /// <summary>
        /// Связь по переменному току.
        /// </summary>
        public void SetCoupleAC();

        /// <summary>
        /// Связь по постоянному току.
        /// </summary>
        public void SetCoupleDC();

        public string GetCouple();

        /// <summary>
        /// Включить входной фильтр.
        /// </summary>
        public void SetFilterOn();

        /// <summary>
        /// Выключить входной фильтр.
        /// </summary>
        public void SetFilterOff();

        /// <summary>
        /// Возвращает текущий статус фильтра.
        /// </summary>
        /// <returns>Статус фильтра on/off.</returns>
        public string GetFilterStatus();
    }

    /// <summary>
    /// Интерфейс выбора условий запуска измерений частотомера (по признакам формы сигнала).
    /// </summary>
    public interface ICounterInputSlopeSetting
    {
        /// <summary>
        /// Запуск по фронту.
        /// </summary>
        public void SetInputSlopePositive();

        /// <summary>
        /// Запуск по спаду.
        /// </summary>
        public void SetInputSlopeNegative();

        public InputSlope Slope { get; }
    }
}