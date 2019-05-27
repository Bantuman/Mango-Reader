/* 

SAKER OCH GÖRA KLART: 
    * kapitel progress sparning
    * importera från webb 
    * support för epub filer
    * support för png filer
    * genres och genre sortering
    * kapitel namn
    * bättre reader
*/ 

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        public string AccessThumbnailPath { get; set; }
        public string AccessChapterPath { get; set; }
        public string AccessSynopsis { get; set; }
        public string AccessTitle { get; set; }
        public Genre[] AccessGenres { get; set; }
        public List<ChapterItem> AccessChapters { get; set; }
        public float AccessRating { get; set; } // ( 0.0f - 1.0f )
        public int AccessChapterProgress { get; set; }

        public ChapterItem GetChapter(string chapterName)
        {
            foreach(ChapterItem tempChapter in AccessChapters)
            {
                if (tempChapter.AccessTitle == chapterName)
                {
                    return tempChapter;
                }
            }
            return null;
        }


        public void LoadData()
        {
             // Ta reda på vilken sida av vilket kapitell
        }

        public void SaveData()
        {
            // Spara vilkens sida av vilket kapitell
        }

        public Mango() => LoadData();
    }

    public interface IBaseListItem
    {
        string AccessTitle { get; set; }
        int AccessCompletion { get; set; }
        int AccessMaxCompletion { get; set; }
    }
    
    public class MangaItem : IBaseListItem
    {
        public string AccessTitle { get; set; }
        public int AccessCompletion { get; set; }
        public int AccessMaxCompletion { get; set; }
    }

    public class ChapterItem : IBaseListItem
    {
        public string AccessManga { get; set; }
        public string AccessTitle { get; set; }
        public string AccessPath { get; set; }
        public int AccessCompletion { get; set; }
        public string GetBindingData { get => AccessManga + "Ö" + AccessTitle; }
        public int AccessMaxCompletion { get; set; }
        public ChapterItem AccessPreviousChapter { get; set; }
        public ChapterItem AccessNextChapter { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Mango myCurrentSelectedMango;
        private List<Mango> myMangos;

        private Mango ParseMango(string[] someChapters, string aThumbnail, string someName, string aSynposis)
        {
            // Hitta vart mangan ska sparas
            string tempBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(tempBasePath + "\\.Mango"))
            {
                Directory.CreateDirectory(tempBasePath + "\\.Mango");
            }
            string tempMangoPath = tempBasePath + "\\.Mango\\" + someName;
            if (!Directory.Exists(tempMangoPath))
            {
                Directory.CreateDirectory(tempMangoPath);
            }

            // Ladda och spara manga thumbnail
            System.Drawing.Image tempImage = System.Drawing.Image.FromFile(aThumbnail);
            tempImage.Save(tempMangoPath + "\\thumbnail.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            tempImage.Dispose();

            // Ladda manga titel, synopsis och spara kapitel
            StreamWriter tempWriter = File.CreateText(tempMangoPath + "\\info.mango");
            tempWriter.WriteLine("n:" + someName + "\ns:" + aSynposis);
            tempWriter.Close();
            for(int i = 0; i < someChapters.Length; ++i)
            {
                File.Copy(someChapters[i], tempMangoPath + "\\" + (i + 1).ToString() + ".pdf", true);
            }
            
            return new Mango()
            {
                AccessTitle = someName,
                AccessSynopsis = aSynposis,
                AccessChapterPath = tempMangoPath,
                AccessThumbnailPath = tempMangoPath + "\\thumbnail.jpg"
            };
        }

        private Mango ParseMango(string someName)
        {
            string tempBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(tempBasePath + "\\.Mango"))
            {
                Directory.CreateDirectory(tempBasePath + "\\.Mango");
            }
            string tempMangoPath = tempBasePath + "\\.Mango\\" + someName;
            using (StreamReader tempMangaReader = new StreamReader(tempMangoPath + "\\info.mango"))
            {
                string tempMangaTitle = tempMangaReader.ReadLine().Split(':')[1];
                string tempMangaSynopsis = tempMangaReader.ReadLine().Split(':')[1];

                return new Mango()
                {
                    AccessTitle = tempMangaTitle,
                    AccessSynopsis = tempMangaSynopsis,
                    AccessChapterPath = tempMangoPath,
                    AccessThumbnailPath = tempMangoPath + "\\thumbnail.jpg"
                };
            }
        }

        private void LoadMango(Mango someMango)
        {
            List<ChapterItem> tempChapterItems = new List<ChapterItem>();
            string[] tempChapterPaths = Directory.GetFiles(someMango.AccessChapterPath, "*.pdf");

            for (int i = 0; i < tempChapterPaths.Length; ++i)
            {
                // Spara kapitel som ChapterItem och formattera namnet korrekt
                string tempChapterName = tempChapterPaths[i].Remove(0, someMango.AccessChapterPath.Length + 1);
                tempChapterName = tempChapterName.Remove(tempChapterName.Length - 4);
                tempChapterItems.Add(new ChapterItem() {AccessPath = tempChapterPaths[i], AccessManga = someMango.AccessTitle, AccessTitle = tempChapterName, AccessCompletion = 0 });
            }

            tempChapterItems.Sort((tempFirstObj, tempSecondObj) =>
            {
                // Sortera kapitlerna beroende på det formaterade namn
                return int.Parse(tempFirstObj.AccessTitle).CompareTo(int.Parse(tempSecondObj.AccessTitle));
            });

            for (int i = 0; i < tempChapterItems.Count; ++i)
            {
                // Hitta föregående och nästa kapitel
                if (i + 1 < tempChapterItems.Count)
                {
                    tempChapterItems[i].AccessNextChapter = tempChapterItems[i + 1];
                }
                if (i - 1 >= 0)
                {
                    tempChapterItems[i].AccessPreviousChapter = tempChapterItems[i - 1];
                }
            }
            someMango.AccessChapters = tempChapterItems;

            // Uppdatera WPF rutan
            List<MangaItem> tempCurrentItems = (List<MangaItem>)MangaList.ItemsSource ?? new List<MangaItem>();
            MangaItem tempListItem = new MangaItem() { AccessTitle = someMango.AccessTitle, AccessCompletion = 50 };
            tempCurrentItems.Add(tempListItem);
            myMangos.Add(someMango);
            MangaList.ItemsSource = tempCurrentItems;
            MangaList.Items.Refresh();
        }

        private Mango FindMango(string someMangoTitle)
        {
            foreach(Mango tempMango in myMangos)
            {
                if (tempMango.AccessTitle == someMangoTitle)
                {
                    return tempMango;
                }
            }
            return null;
        }

        private BitmapImage GetBitmap(Uri someUri)
        {
            // Nödvändigt eftersom bilden måste cachas, så att orginalet kan tas bort
            BitmapImage tempBitmap = new BitmapImage();
            tempBitmap.CacheOption = BitmapCacheOption.OnLoad;
            tempBitmap.BeginInit();
            tempBitmap.UriSource = someUri;
            tempBitmap.EndInit();
            return tempBitmap;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object someSender, MouseButtonEventArgs someEventArg)
        {
            ListViewItem tempListViewItem = someSender as ListViewItem;
            if (tempListViewItem != null)
            {
                Mango tempSelectedMango = FindMango((tempListViewItem.Content as IBaseListItem).AccessTitle);
                if (tempSelectedMango != null)
                {
                    // Uppdatera WPF rutan
                    MangoSynopsisText.Text = tempSelectedMango.AccessSynopsis;
                    MangoThumbnail.Source = GetBitmap(new Uri(tempSelectedMango.AccessThumbnailPath));
                    MangoThumbnail.UpdateLayout();
                    MangoSynopsisBorder.Margin = new Thickness(MangoThumbnail.ActualWidth + 11, 0, 0, 0);

                    ChapterList.ItemsSource = tempSelectedMango.AccessChapters;
                    myCurrentSelectedMango = tempSelectedMango;
                }
            }
        }

        public MainWindow()
        {
            myMangos = new List<Mango>();
            InitializeComponent();

            // Skapa mapp i %appdata% för att spara all manga, borde nog ändra till Documents i framtiden
            string tempBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(tempBasePath + "\\.Mango"))
            {
                Directory.CreateDirectory(tempBasePath + "\\.Mango");
            }
            tempBasePath += "\\.Mango";
            string[] tempAllMangoPaths = Directory.GetDirectories(tempBasePath);
            foreach (string tempMangoPath in tempAllMangoPaths)
            {
                string tempMangoName = tempMangoPath.Substring(tempBasePath.Length + 1);
                LoadMango(ParseMango(tempMangoName));
            }
        }

        private void Button_Click(object someSender, RoutedEventArgs someEventArg)
        {
            // Manga datan sparas i sen string på följande vis: MangaTitleÖMangaSynopsis
            string[] tempChapterData = ((someSender as Button).Parent as Grid).Tag.ToString().Split('Ö');
            string tempMangaTitle = tempChapterData[0];
            string tempChapterTitle = tempChapterData[1];
            Mango tempManga = FindMango(tempMangaTitle);

            ReaderWindow tempReader = new ReaderWindow(tempManga, tempChapterTitle);
            tempReader.Title = tempMangaTitle + " : " + tempChapterTitle;
            tempReader.Show();
        }

        private void Window_Closed()
        {
            // Stäng ner alla gömda läsar fönster och import fönster
            Environment.Exit(0);
        }

        private void ImportManga_Click(object someSender, EventArgs someEventArg)
        {
            ImportWindow tempImportWindow = (((someSender as Button).Parent as Grid).Parent as ImportWindow);
            string[] tempFilePaths = new string[] { };
            using (System.Windows.Forms.OpenFileDialog tempOpenChaptersDialog = new System.Windows.Forms.OpenFileDialog())
            {
                tempOpenChaptersDialog.InitialDirectory = "C:\\";
                tempOpenChaptersDialog.Multiselect = true;
                tempOpenChaptersDialog.Filter = "PDF Filer (*.pdf)|*.pdf"; // Borde lägga till support för EPUB Filer (*.epub)|*.epub|
                tempOpenChaptersDialog.FilterIndex = 1;
                tempOpenChaptersDialog.RestoreDirectory = true;

                if (tempOpenChaptersDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (System.Windows.Forms.OpenFileDialog tempOpenThumbnailDialog = new System.Windows.Forms.OpenFileDialog())
                    {
                        tempOpenThumbnailDialog.InitialDirectory = "C:\\";
                        tempOpenThumbnailDialog.Filter = "JPG Filer (*.jpg)|*.jpg"; // Borde lägga till support för PNG Filer (*.png)|*.png|
                        tempOpenThumbnailDialog.FilterIndex = 1;
                        tempOpenThumbnailDialog.RestoreDirectory = true;

                        if (tempOpenThumbnailDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            /* Skapa och lägg till en manga 
                             * med kapitel (openChaptersDialog.FileNames) 
                             * med thumbnail (openThumbnailDialog.FileName)
                             * med titel (importWindow.MangaTitle)
                             * med synopsen (importWindow.MangaSynopsis)
                             */
                            string tempMangaSynopsis = new TextRange(tempImportWindow.MangaSynopsis.Document.ContentStart, tempImportWindow.MangaSynopsis.Document.ContentEnd).Text;
                            LoadMango(ParseMango(tempOpenChaptersDialog.FileNames, tempOpenThumbnailDialog.FileName, tempImportWindow.MangaTitle.Text, tempMangaSynopsis));

                            // Sedan gömm import fönstret och stäng ner alla filöppnings processer
                            tempImportWindow.Hide();
                            tempOpenThumbnailDialog.Dispose();
                            tempOpenChaptersDialog.Dispose();
                        }
                    }
                }
            }
        }

        private void MenuItem_Click()
        {
            ImportWindow tempWindow = new ImportWindow();
            tempWindow.Show();
            tempWindow.NextButton.Click += ImportManga_Click;
        }

        private void MangoThumbnail_MouseLeftButtonUp()
        {
            MangoThumbnail.Source = null;
            MangoThumbnail.UpdateLayout();
            using (System.Windows.Forms.OpenFileDialog tempOpenFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                tempOpenFileDialog.InitialDirectory = "C:\\";
                tempOpenFileDialog.Filter = "JPEG Filer (*.jpg)|*.jpg"; // Borde lägga till support för PNG Filer (*.png)|*.png|
                tempOpenFileDialog.FilterIndex = 1;
                tempOpenFileDialog.RestoreDirectory = true;

                if (tempOpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Image tempImage = System.Drawing.Image.FromFile(tempOpenFileDialog.FileName);

                    GC.Collect();
                    GC.WaitForPendingFinalizers(); // GarbageCollection för att kunna ta bort bilden

                    File.Delete(myCurrentSelectedMango.AccessThumbnailPath);
                    tempImage.Save(myCurrentSelectedMango.AccessThumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    tempImage.Dispose();

                    // Uppdatera WPF rutan
                    MangoThumbnail.Source = GetBitmap(new Uri(myCurrentSelectedMango.AccessThumbnailPath));
                    MangoThumbnail.UpdateLayout();
                    MangoSynopsisBorder.Margin = new Thickness(MangoThumbnail.ActualWidth + 11, 0, 0, 0);
                }
            }
        }
    }
}