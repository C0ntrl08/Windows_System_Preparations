using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanguagePacksCheckerAndSetter.Views
{
    /// <summary>
    /// Interaction logic for CheckView.xaml
    /// </summary>
    public partial class CheckView : UserControl
    {
        public CheckView()
        {
            InitializeComponent();
            OperationLog_CollectionChanged();
        }


        private void OperationLog_CollectionChanged()
        {
            if (DataContext is ViewModels.CheckViewModel vm)
            {
                vm.OperationLog.CollectionChanged += (s, e) =>
                {
                    if (LogList.Items.Count > 0)
                    {
                        LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
                    }
                };
            }
        }
    }
}