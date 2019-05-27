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
        private int myCurrentPage;
        private int myAmountOfPages;
        private Mango myCurrentManga;
        private string myCurrentChapter;

        public ReaderWindow(Mango someManga, string someChapter)
        {
            InitializeComponent();
            PreviewKeyDown += Reader_KeyDown;
            LoadManga(someManga, someChapter);
            MovePage(1);
        }

        private void LoadManga(Mango someManga, string someChapter)
        {
            myCurrentPage = 0;
            myCurrentManga = someManga;
            myCurrentChapter = someChapter;
            myAmountOfPages = PdfDocument.Load(someManga.GetChapter(someChapter).AccessPath).PageCount;
        }

        private void Reader_KeyDown(object someSender, KeyEventArgs someKeyEventArg)
        {
            if (someKeyEventArg.Key == Key.Escape)
            {
                Hide();
            }
            if (someKeyEventArg.Key == Key.Right)
            {
                MovePage(1);
            }
            if (someKeyEventArg.Key == Key.Left)
            {
                MovePage(-1);
            }
        }

        private void MovePage(int anIncrement)
        {
            myCurrentPage = myCurrentPage + anIncrement;
            if (myCurrentPage > myAmountOfPages)
            {
                ChapterItem nextChapter = myCurrentManga.GetChapter(myCurrentChapter).AccessNextChapter;
                if (nextChapter != null)
                {
                    myCurrentChapter = nextChapter.AccessTitle;
                }
                myCurrentPage = 0;
            }
            if (myCurrentPage < 0)
            {
                ChapterItem myPreviousChapter = myCurrentManga.GetChapter(myCurrentChapter).AccessPreviousChapter;
                if (myPreviousChapter != null)
                {
                    myCurrentChapter = myPreviousChapter.AccessTitle;
                    myCurrentPage = PdfDocument.Load(myPreviousChapter.AccessPath).PageCount - 1;
                }
                else
                {
                    myCurrentPage = 0;
                }
            }

            Title = myCurrentManga.AccessTitle + " : " + myCurrentChapter;
            MangaTitle.Text = "     " + Title;
            RenderPage(this, myCurrentManga.GetChapter(myCurrentChapter).AccessPath, myCurrentPage, new Size(1024, 1024));
        }

        private System.Drawing.Image GetPageImage(int aPageNumber, Size someSize, PdfDocument aDocument, int someDpi)
        {
            return aDocument.Render(aPageNumber - 1, (int)someSize.Width, (int)someSize.Height, someDpi, someDpi, PdfRotation.Rotate0, PdfRenderFlags.Annotations);
        }

        private void RenderPage(ReaderWindow someReaderWindow, string somePdfPath, int aPageNumber, Size someSize)
        {
            using (var tempDocument = PdfDocument.Load(somePdfPath))
            using (var tempImage = GetPageImage(aPageNumber, someSize, tempDocument, 150))
            using (var tempMemoryStream = new MemoryStream())
            {
                tempImage.Save(tempMemoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                tempMemoryStream.Seek(0, SeekOrigin.Begin);

                var tempBitmapImage = new BitmapImage();
                tempBitmapImage.BeginInit();
                tempBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                tempBitmapImage.StreamSource = tempMemoryStream;
                tempBitmapImage.EndInit();

                Width = tempBitmapImage.Width;
                Height = tempBitmapImage.Height;

                myAmountOfPages = tempDocument.PageCount;
                someReaderWindow.MangaDisplay.Source = tempBitmapImage;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
