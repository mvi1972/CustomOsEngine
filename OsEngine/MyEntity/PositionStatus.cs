using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    /// <summary>
    /// перечисление типов статусов позиции
    /// </summary>
    public enum PositionStatus
    {
        /// <summary>
        /// не назначен
        /// </summary>
        None,

        /// <summary>
        /// открывается
        /// </summary>
        Opening,

        /// <summary>
        /// закрыта
        /// </summary>
        Done,

        /// <summary>
        /// ошибка
        /// </summary>
        OpeningFail,

        /// <summary>
        /// открыта
        /// </summary>
        Open,

        /// <summary>
        /// закрывается
        /// </summary>
        Closing,

        /// <summary>
        /// ошибка на закрытии
        /// </summary>
        ClosingFail,

        /// <summary>
        /// удалена
        /// </summary>
        Deleted
    }

}
