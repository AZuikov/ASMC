﻿using System.Drawing;

namespace AP.Reports.AutoDocumets
{
    public abstract class Document
    {
        /// <summary>
        /// Предоставляет паттерн определяющий допустимое наименование Bookmark
        /// </summary>
        protected static string PatternBookmark
            => @"[a-z, A-Z, а-я, А-Я][_, a-z, A-Z, а-я, А-Я]*";

        /// <summary>
        /// Предостовляет паттерн для поиска текста
        /// </summary>
        /// <param name="sFind"></param>
        /// <returns></returns>
        protected static string PatternFindText(string sFind)
        {
            return @"\b" + sFind + @"\b";
        }

        /// <summary>
        /// Набор правил для проверки и заливки таблици
        /// </summary>
        public struct ConditionalFormatting
        {
            /// <summary>
            /// Перечень допустимых условий
            /// </summary>
            public enum Conditions
            {
                /// <summary>
                /// Меньше
                /// </summary>
                Less,

                /// <summary>
                /// Больше
                /// </summary>
                More,

                /// <summary>
                /// Равно
                /// </summary>
                Equal,

                /// <summary>
                /// Меньше или равно
                /// </summary>
                LessOrEqual,

                /// <summary>
                /// Больше или равно
                /// </summary>
                MoreOrEqual,

                /// <summary>
                /// Не равно
                /// </summary>
                NotEqual
            }

            /// <summary>
            /// Область заливки
            /// </summary>
            public enum RegionAction
            {
                /// <summary>
                /// Заливка строки
                /// </summary>
                Row,

                /// <summary>
                /// Заливка только ячейки
                /// </summary>
                Cell
            }

            /// <summary>
            /// Имя столбца для проверки
            /// </summary>
            public string NameColumn { get; set; }

            /// <summary>
            /// Тип условия
            /// </summary>
            public Conditions Condition { get; set; }

            /// <summary>
            /// Значения условия
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Цвет заливки для выделения.
            /// </summary>
            public Color Select { get; set; }
            /// <summary>
            /// Цвет заливки по умолчанию.
            /// </summary>
            public Color Default { get; set; }
            


            /// <summary>
            /// Область заливки
            /// </summary>
            public RegionAction Region { get; set; }
        }
    }
}