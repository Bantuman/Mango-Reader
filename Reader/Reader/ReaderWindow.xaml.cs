using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Reader
{
    /// <summary>
    /// Interaction logic for ReaderWindow.xaml
    /// </summary>
    public partial class ReaderWindow : Window
    {
        private int currentPage;
        private int amountOfPages;
        private Mango currentManga;
        private string currentChapter;

        public ReaderWindow(Mango manga, string someChapter)
        {
            InitializeComponent();
            PreviewKeyDown += Reader_KeyDown;
            LoadManga(manga, someChapter);
            MovePage(1);
        }

        private void LoadManga(Mango someManga, string someChapter)
        {
            currentPage = 0;
            currentManga = someManga;
            currentChapter = someChapter;
            amountOfPages = PdfDocument.Load(someManga.GetChapter(someChapter).Path).PageCount;
        }

        private void Reader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
            if (e.Key == Key.Right)
            {
                MovePage(1);
            }
            if (e.Key == Key.Left)
            {
                MovePage(-1);
            }
        }

        private void MovePage(int increment)
        {
            currentPage = currentPage + increment;
            if (currentPage > amountOfPages)
            {
                ChapterItem nextChapter = currentManga.GetChapter(currentChapter).NextChapter;
                if (nextChapter != null)
                {
                    currentChapter = nextChapter.Title;
                }
                currentPage = 0;
            }
            if (currentPage < 0)
            {
                ChapterItem previousChapter = currentManga.GetChapter(currentChapter).PreviousChapter;
                if (previousChapter != null)
                {
                    currentChapter = previousChapter.Title;
                    currentPage = PdfDocument.Load(previousChapter.Path).PageCount - 1;
                }
                else
                {
                    currentPage = 0;
                }
            }

            Title = currentManga.Title + " : " + currentChapter;
            MangaTitle.Text = "     " + Title;
            RenderPage(this, currentManga.GetChapter(currentChapter).Path, currentPage, new Size(1024, 1024));
        }

        private System.Drawing.Image GetPageImage(int pageNumber, Size size, PdfDocument document, int dpi)
        {
            return document.Render(pageNumber - 1, (int)size.Width, (int)size.Height, dpi, dpi, PdfRotation.Rotate0, PdfRenderFlags.Annotations);
        }

        private void RenderPage(ReaderWindow reader, string pdfPath, int pageNumber, Size size)
        {
            using (var document = PdfDocument.Load(pdfPath))
            using (var image = GetPageImage(pageNumber, size, document, 150))
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                Width = bitmapImage.Width;
                Height = bitmapImage.Height;

                amountOfPages = document.PageCount;
                reader.MangaDisplay.Source = bitmapImage;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
