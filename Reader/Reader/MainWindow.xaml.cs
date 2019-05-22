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
using System.Timers;
using System.Diagnostics;
using System.IO;


namespace Reader
{
    public enum Genre
    {
        Comedy,
        SliceOfLife,
        Shonen,
        Isekai
    }

    public class Mango
    {
        public string ThumbnailPath { get; set; }
        public string ChapterPath { get; set; }
        public string Synopsis { get; set; }
        public string Title { get; set; }
        public Genre[] Genres { get; set; }
        public List<ChapterItem> Chapters { get; set; }
        public float Rating { get; set; } // ( 0.0f - 1.0f )
        public int ChapterProgress { get; set; }

        public ChapterItem GetChapter(string chapterName)
        {
            foreach(ChapterItem chapter in Chapters)
            {
                if (chapter.Title == chapterName)
                {
                    return chapter;
                }
            }
            return null;
        }


        public void LoadData()
        {
             // load progress from some file
        }

        public void SaveData()
        {
            // save progress to some file
        }

        public Mango() => LoadData();
    }

    public interface IBaseListItem
    {
        string Title { get; set; }
        int Completion { get; set; }
        int MaxCompletion { get; set; }
    }
    
    public class MangaItem : IBaseListItem
    {
        public string Title { get; set; }
        public int Completion { get; set; }
        public int MaxCompletion { get; set; }
    }

    public class ChapterItem : IBaseListItem
    {
        public string Manga { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int Completion { get; set; }
        public string BindingData { get => Manga + "Ö" + Title; }
        public int MaxCompletion { get; set; }
        public ChapterItem PreviousChapter { get; set; }
        public ChapterItem NextChapter { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Mango currentSelectedMango;
        private List<Mango> allMyMangos;

        private Mango ParseMango(string[] chapters, string name, string synposis)
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(basePath + "\\.Mango"))
            {
                Directory.CreateDirectory(basePath + "\\.Mango");
            }
            string mangoPath = basePath + "\\.Mango\\" + name;
            if (!Directory.Exists(mangoPath))
            {
                Directory.CreateDirectory(mangoPath);
            }
           
            System.Drawing.Image image = System.Drawing.Image.FromFile("C:/Users/Administratör.5CG62401YF/Documents/GitHub/Mango-Reader/Reader/Reader/res/missing.jpg"); //"pack://application:,,,/res/missing"
            image.Save(mangoPath + "\\thumbnail.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            StreamWriter writer = File.CreateText(mangoPath + "\\info.mango");
            writer.WriteLine("n:" + name + "\ns:" + synposis);
            writer.Close();
            for(int i = 0; i < chapters.Length; ++i)
            {
                File.Copy(chapters[i], mangoPath + "\\" + (i + 1).ToString() + ".pdf", true);
            }
            
            return new Mango()
            {
                Title = name,
                Synopsis = synposis,
                ChapterPath = mangoPath,
                ThumbnailPath = mangoPath + "\\thumbnail.jpg"
            };
        }

        private Mango ParseMango(string name)
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(basePath + "\\.Mango"))
            {
                Directory.CreateDirectory(basePath + "\\.Mango");
            }
            string mangoPath = basePath + "\\.Mango\\" + name;
            using (StreamReader mangaRead = new StreamReader(mangoPath + "\\info.mango"))
            {
                string mangaTitle = mangaRead.ReadLine().Split(':')[1];
                string mangaSynopsis = mangaRead.ReadLine().Split(':')[1];

                return new Mango()
                {
                    Title = mangaTitle,
                    Synopsis = mangaSynopsis,
                    ChapterPath = mangoPath,
                    ThumbnailPath = mangoPath + "\\thumbnail.jpg"
                };
            }
        }

        private void LoadMango(Mango someMango)
        {
            List<ChapterItem> chapterItems = new List<ChapterItem>();
            string[] chapterPaths = Directory.GetFiles(someMango.ChapterPath, "*.pdf");
            for (int i = 0; i < chapterPaths.Length; ++i)
            {
                string chapterName = chapterPaths[i].Remove(0, someMango.ChapterPath.Length + 1);
                chapterName = chapterName.Remove(chapterName.Length - 4);
                chapterItems.Add(new ChapterItem() {Path = chapterPaths[i], Manga = someMango.Title, Title = chapterName, Completion = 0 });
            }
            chapterItems.Sort((firstObj, secondObj) =>
            {
                return int.Parse(firstObj.Title).CompareTo(int.Parse(secondObj.Title));
            });
            for (int i = 0; i < chapterItems.Count; ++i)
            {
                if (i + 1 < chapterItems.Count)
                {
                    chapterItems[i].NextChapter = chapterItems[i + 1];
                }
                if (i - 1 >= 0)
                {
                    chapterItems[i].PreviousChapter = chapterItems[i - 1];
                }
            }
            someMango.Chapters = chapterItems;

            List<MangaItem> currentItems = (List<MangaItem>)MangaList.ItemsSource ?? new List<MangaItem>();
            MangaItem newListItem = new MangaItem() { Title = someMango.Title, Completion = 50 };
            currentItems.Add(newListItem);
            allMyMangos.Add(someMango);
            MangaList.ItemsSource = currentItems;
            MangaList.Items.Refresh();
        }

        private Mango GetMango(string mangoTitle)
        {
            foreach(Mango mango in allMyMangos)
            {
                if (mango.Title == mangoTitle)
                {
                    return mango;
                }
            }
            return null;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                Mango selectedMango = GetMango((item.Content as IBaseListItem).Title);
                if (selectedMango != null)
                {
                    MangoSynopsisText.Text = selectedMango.Synopsis;
                    MangoThumbnail.Source = new BitmapImage(new Uri(selectedMango.ThumbnailPath));
                    MangoThumbnail.UpdateLayout();
                    ChapterList.ItemsSource = selectedMango.Chapters;
                    currentSelectedMango = selectedMango;
                    MangoSynopsisBorder.Margin = new Thickness(MangoThumbnail.ActualWidth + 11, 0, 0, 0);
                }
            }
        }

        public MainWindow()
        {
            allMyMangos = new List<Mango>();
            InitializeComponent();

            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(basePath + "\\.Mango"))
            {
                Directory.CreateDirectory(basePath + "\\.Mango");
            }
            basePath += "\\.Mango";
            string[] allMyMangoPaths = Directory.GetDirectories(basePath);
            foreach (string mangoPath in allMyMangoPaths)
            {
                string mangoName = mangoPath.Substring(basePath.Length + 1);
                LoadMango(ParseMango(mangoName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] chapterData = ((sender as Button).Parent as Grid).Tag.ToString().Split('Ö');
            string mangaTitle = chapterData[0];
            string chapterTitle = chapterData[1];
            Mango manga = GetMango(mangaTitle);

            ReaderWindow reader = new ReaderWindow(manga, chapterTitle);
            reader.Title = mangaTitle + " : " + chapterTitle;
            reader.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void ImportManga_Click(object sender, EventArgs e)
        {
            ImportWindow importWindow = (((sender as Button).Parent as Grid).Parent as ImportWindow);
            string[] filePaths = new string[] { };
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "PDF Filer (*.pdf)|*.pdf"; // EPUB Filer (*.epub)|*.epub|
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadMango(ParseMango(openFileDialog.FileNames, importWindow.MangaTitle.Text, new TextRange(importWindow.MangaSynposis.Document.ContentStart, importWindow.MangaSynposis.Document.ContentEnd).Text));
                    importWindow.Hide();
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportWindow window = new ImportWindow();
            window.Show();
            window.NextButton.Click += ImportManga_Click;
        }

        private void MangoThumbnail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "JPEG Filer (*.jpg)|*.jpg"; // EPUB Filer (*.epub)|*.epub|
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                    //image.Save(currentSelectedMango.ThumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    MangoThumbnail.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                    MangoThumbnail.UpdateLayout();
                }
            }
        }
    }
}