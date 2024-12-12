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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;
using System.IO;


namespace DailyPlanner
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Task> Tasks { get; set; }
        private ObservableCollection<Task> FilteredTasks { get; set; }
        public ObservableCollection<Task> CompletedTasks { get; set; }
        private Task selectedTask;

        private string tasksFilePath = "tasks.json";
        private string completedFileTasks = "completedTasks.json";

        public MainWindow()
        {
            InitializeComponent();

            Tasks = new ObservableCollection<Task>();
            CompletedTasks = new ObservableCollection<Task>();

            TaskList.ItemsSource = Tasks;
            CompletedTaskList.ItemsSource = CompletedTasks;

            Tasks.CollectionChanged += Tasks_CollectionChanged;
            CompletedTasks.CollectionChanged += Tasks_CollectionChanged;
            FilteredTasks = new ObservableCollection<Task>(Tasks);

            LoadTasks();
            LoadCompletedTasks();

            TaskList.ItemsSource = FilteredTasks;

           
        }


        private void LoadTasks()
        {
            if (File.Exists(tasksFilePath))
            {
                var json = File.ReadAllText(tasksFilePath);
                var loadedTasks = JsonConvert.DeserializeObject<ObservableCollection<Task>>(json);

                if (loadedTasks != null)
                {
                    foreach (var task in loadedTasks)
                    {
                        Tasks.Add(task);
                    }
                }
            }

           
            FilteredTasks = new ObservableCollection<Task>(Tasks);
            TaskList.ItemsSource = FilteredTasks;
        }

        private void LoadCompletedTasks()
        {
            if (File.Exists(completedFileTasks))
            {
                var json = File.ReadAllText(completedFileTasks);
                var loadedTasks = JsonConvert.DeserializeObject<ObservableCollection<Task>>(json);

                if (loadedTasks != null)
                {
                    foreach (var task in loadedTasks)
                    {
                        CompletedTasks.Add(task);
                    }
                }
            }

           
            FilteredTasks = new ObservableCollection<Task>(Tasks);
            TaskList.ItemsSource = FilteredTasks;
        }

        private void SaveTasks()
        {
            var json = JsonConvert.SerializeObject(Tasks, Formatting.Indented);
            File.WriteAllText(tasksFilePath, json);
        }

        private void SaveCompletedTasks()
        {
            var json = JsonConvert.SerializeObject(CompletedTasks, Formatting.Indented);
            File.WriteAllText(completedFileTasks, json);
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchInput.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                ResetSearch();
                return;
            }

           
            var results = Tasks.Where(task => task.Description.ToLower().Contains(query)).ToList();

            FilteredTasks.Clear();
            foreach (var task in results)
            {
                FilteredTasks.Add(task);
                TaskList.ItemsSource = FilteredTasks;
            }

            ResetSearchButton.Visibility = Visibility.Visible;
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetSearch();
        }

        private void ResetSearch()
        {
            FilteredTasks.Clear();
            foreach (var task in Tasks)
            {
                FilteredTasks.Add(task);
            }

            SearchInput.Clear();
            ResetSearchButton.Visibility = Visibility.Collapsed; // Прячем кнопку сброса
        }
        private void TaskDate_Loaded(object sender, RoutedEventArgs e)
        {
            if (TaskDate.Template.FindName("PART_Calendar", TaskDate) is Calendar calendar)
            {
                calendar.Loaded += (s, ev) => HighlightTaskDates(calendar);
                calendar.DisplayDateChanged += (s, ev) => HighlightTaskDates(calendar);
            }
        }

        private void HighlightTaskDates(Calendar calendar)
        {
            foreach (var dayButton in FindVisualChildren<CalendarDayButton>(calendar))
            {
                if (dayButton.DataContext is DateTime date && Tasks.Any(task => task.Date.Date == date.Date))
                {
                    dayButton.Background = Brushes.Red; 
                    dayButton.Foreground = Brushes.White; 
                }
                else
                {
                    dayButton.Background = Brushes.Transparent; 
                    dayButton.Foreground = Brushes.Black; 
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T childOfT)
                {
                    yield return childOfT;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            return;
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var taskToComplete = button.Tag as Task;

            if (taskToComplete != null)
            {
                Tasks.Remove(taskToComplete);
                FilteredTasks.Remove(taskToComplete);
                taskToComplete.IsCompleted = true;
                CompletedTasks.Add(taskToComplete);

                SaveTasks();
                SaveCompletedTasks();
            }
        }
        private void DeleteCompletedTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var taskToRemove = button.Tag as Task;
            CompletedTasks.Remove(taskToRemove);

            SaveTasks();
            SaveCompletedTasks();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TaskInput.Text) && TaskDate.SelectedDate.HasValue)
            {
                Tasks.Add(new Task { Description = TaskInput.Text, Date = TaskDate.SelectedDate.Value, IsCompleted = false });
                ClearInputFields();
                TaskList.ItemsSource = Tasks;

                SaveTasks();
                SaveCompletedTasks();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните задачу и выберите дату.");
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var taskToRemove = button.Tag as Task;
            Tasks.Remove(taskToRemove);
            FilteredTasks.Remove(taskToRemove);

            SaveTasks();
            SaveCompletedTasks();
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            selectedTask = button.Tag as Task;

            if (selectedTask != null)
            {
                TaskInput.Text = selectedTask.Description;
                TaskDate.SelectedDate = selectedTask.Date;

                AddButton.Visibility = Visibility.Collapsed;
                UpdateButton.Visibility = Visibility.Visible;

                SaveTasks();
                SaveCompletedTasks();
            }
        }

        
        private void UpdateTask_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTask != null && !string.IsNullOrEmpty(TaskInput.Text) && TaskDate.SelectedDate.HasValue)
            {
                selectedTask.Description = TaskInput.Text;
                selectedTask.Date = TaskDate.SelectedDate.Value;

                TaskList.Items.Refresh();

                ClearInputFields();
                AddButton.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Collapsed;

                SaveTasks();
                SaveCompletedTasks();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните задачу и выберите дату.");
            }
        }

        
        private void ClearInputFields()
        {
            TaskInput.Clear();
            TaskDate.SelectedDate = null;
        }
    }

    public class Task
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
    }
}
