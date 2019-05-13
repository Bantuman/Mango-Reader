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
        private List<Mango> allMyMangos;

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
                    MangoSynopsisBorder.Margin = new Thickness(MangoThumbnail.ActualWidth + 11, 0, 0, 0);
                }
            }
        }

        public MainWindow()
        {
            allMyMangos = new List<Mango>();
            InitializeComponent();

            LoadMango(ParseMango("Yotsubato&!"));
            LoadMango(ParseMango("Test"));
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
    }
}
