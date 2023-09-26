using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using WPFUtilities.ComponentModel;

namespace WPFUtilities.Converters
{
    /// <summary>
    /// convert a size to a double according a percent ratio (0 to 1)
    /// </summary>
    public class SizePercentConverter :
        Singleton<SizePercentConverter>,
        IMultiValueConverter
    {
        /// <summary>
        /// convert a size to a double according a percent ratio (0 to 1)
        /// </summary>
        /// <param name="values">size = values[0], ratio = values[1]</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns>the size mul the ratio</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var res0 = System.Convert.ToDouble(values[0]);
            var res1 = System.Convert.ToDouble(values[1]);
            double resultFinal;

            // Vypočet polohy posledního pixelu obdélníku vzhledem k celkové šířce
            var polohaObdelniku = res0 * res1;

            // Pokud je aplikace ve FullScreen, použij tuto logiku:
            // Tímto se bude volume meter pohybovat po celých obdélnících
            // Pouze pro rozlišení 1280*400 px
            if (res0 == 620)//(Application.Current.MainWindow.WindowStyle == WindowStyle.None)
            {
                // Délka sloupečku a mezery
                var sloupecDelka = 26.200000000000045;
                var mezera = 4.0;

                // Celková šířka plochy pro vykreslovaný obdélník
                var celkovaSirka = (sloupecDelka + mezera);

                // Vypočet, kolik sloupečků zabere plocha obdélníku
                var sloupceZabraneObdelnikem = polohaObdelniku / celkovaSirka;

                // Zaokrouhlení na nejbližší celé číslo
                var pocetSloupcuZabranych = (int)Math.Ceiling(sloupceZabraneObdelnikem);

                resultFinal = pocetSloupcuZabranych * celkovaSirka;
            }
            else
            {
                resultFinal = Math.Max(0, polohaObdelniku);
            }

            return resultFinal;
        }

        /// <summary>
        /// convert back
        /// </summary>
        /// <exception cref="NotImplementedException">not implemented</exception>
        /// <param name="value">value</param>
        /// <param name="targetTypes">target types</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
