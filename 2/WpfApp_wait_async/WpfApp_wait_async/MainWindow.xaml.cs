using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_wait_async
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            listBox.Items.Add("MainWindow");
        }

        async Task WaitAsync()
        {
            listBox.Items.Add($"This await will capture the current context ...{Environment.CurrentManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            listBox.Items.Add($"and will attempt to resume the method here in that context.{Environment.CurrentManagedThreadId}");
        }
        void Deadlock()
        {
            listBox.Items.Add("Start the delay.");
            Task task = WaitAsync();
            listBox.Items.Add("Synchronously block, waiting for the async method to complete.");
            task.Wait();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Deadlock();
            
        }
    }
}