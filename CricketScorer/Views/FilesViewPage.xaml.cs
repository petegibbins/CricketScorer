namespace CricketScorer.Views;

public partial class FilesViewPage : ContentPage
{
    public FilesViewPage()
    {
        InitializeComponent();
        LoadFileList();
    }

    private void LoadFileList()
    {
        var path = FileSystem.AppDataDirectory;
        var files = Directory.GetFiles(path, "*.json")
                     .Select(f => new FileInfo(f))
                     .OrderByDescending(f => f.LastWriteTime)
                     .Select(f => f.Name)
                     .ToList();
        FileListView.ItemsSource = files;
    }

    private void OnFileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedFile)
        {
            string fullPath = Path.Combine(FileSystem.AppDataDirectory, selectedFile);

            if (File.Exists(fullPath))
            {
                string contents = File.ReadAllText(fullPath);
                FileContentsEditor.Text = contents;
            }
        }
    }
}