using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using VideoConvert.Core;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Helpers.IMDB;
using VideoConvert.Core.Helpers.TheMovieDB;
using log4net;

namespace VideoConvert.Windows.TheMovieDB
{
    /// <summary>
    /// Interaktionslogik für MovieDBInfo.xaml
    /// </summary>
    public partial class MovieDBInfo : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MovieDBInfo));

        private const string MovieDBApiKey = "3c0a6fc7bb8fea5432a4e21ec32be907";
        private const string TheTVDBApiKey = "1DBEA8A1430711B7";

        public string MovieName;

        public MovieEntry ResultMovieData;
        public string ResultBackdropImage;
        public string ResultPosterImage;

        private readonly List<MovieDBPosterImage> _postersList;
        private readonly List<MovieDBImageInfo> _backdropsList;
        private readonly MovieDBCastList _castList;
        
        public MovieDBInfo()
        {
            InitializeComponent();
            _postersList = new List<MovieDBPosterImage>();
            _backdropsList = new List<MovieDBImageInfo>();
            _castList = new MovieDBCastList();
        }

        private void MovieInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MovieName))
                MediaTitleInfo.Text = MovieName;

            SearchLanguage.ItemsSource = MovieDBLanguages.LangList;
            SearchLanguage.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastLanguage)
                                               ? AppSettings.MovieDBLastLanguage
                                               : "en";

            RatingCountry.ItemsSource = MovieDBCertCountries.CountryList;
            RatingCountry.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastRatingCountry)
                                              ? AppSettings.MovieDBLastRatingCountry
                                              : "us";
        }

        private void MediaInfoLoadButton_Click(object sender, RoutedEventArgs e)
        {
            string searchMedia = MediaTitleInfo.Text;
            if (string.IsNullOrEmpty(searchMedia)) return;

            string selectedLang = (string) SearchLanguage.SelectedValue;
            if (string.IsNullOrEmpty(selectedLang))
                selectedLang = "en";
            
            string selectedCertCountry = (string) RatingCountry.SelectedValue;
            if (string.IsNullOrEmpty(selectedCertCountry))
                selectedCertCountry = "us";

            string fallbackLang = AppSettings.MovieDBLastFallbackLanguage;
            string fallbackCertCountry = AppSettings.MovieDBLastFallbackRatingCountry;

            #region MovieDB Client configuration

            TMDbClient client = new TMDbClient(MovieDBApiKey);
            FileInfo configXml = new FileInfo("TMDbconfig.xml");
            if (configXml.Exists && configXml.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-1))
            {
                Log.Info("TMDbClient: Using stored config");
                string xml = File.ReadAllText(configXml.FullName, Encoding.Unicode);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                client.SetConfig(TMDbSerializer.Deserialize<TMDbConfig>(xmlDoc));
            }
            else
            {
                Log.Info("TMDbClient: Getting new config");
                client.GetConfig();

                Log.Info("TMDbClient: Storing config");
                XmlDocument xmlDoc = TMDbSerializer.Serialize(client.Config);
                File.WriteAllText(configXml.FullName, xmlDoc.OuterXml, Encoding.Unicode);
            }

            #endregion

            SearchContainer<SearchMovie> movieList = client.SearchMovie(searchMedia, selectedLang);
            if (movieList == null || movieList.TotalResults <= 0) 
                movieList = client.SearchMovie(searchMedia, fallbackLang);

            if (movieList == null || movieList.TotalResults <= 0) return;

            SearchMovie resultMovie = new SearchMovie();
            if (movieList.TotalResults > 1)
            {
                MovieDBMultipleSelection selectWindow = new MovieDBMultipleSelection
                    {
                        Owner = this,
                        SearchResults = movieList
                    };
                if (selectWindow.ShowDialog() == true)
                    resultMovie = selectWindow.SelectionResult;
            }
            else
                resultMovie = movieList.Results.First();

            if (resultMovie.Id == 0) return;

            Movie searchResult = client.GetMovie(resultMovie.Id, selectedLang) ??
                                 client.GetMovie(resultMovie.Id, fallbackLang);

            if (searchResult == null) return;

            ImagesWithId imageList = client.GetMovieImages(resultMovie.Id);
            Casts movieCasts = client.GetMovieCasts(resultMovie.Id);
            KeywordsContainer movieKeywords = client.GetMovieKeywords(resultMovie.Id);
            Trailers movieTrailers = client.GetMovieTrailers(resultMovie.Id);
            Releases movieReleases = client.GetMovieReleases(resultMovie.Id);

            MovieTitle.Text = searchResult.Title;
            MovieOriginalTitle.Text = searchResult.OriginalTitle;

            Genre.Text = searchResult.Genres != null
                             ? string.Join(" / ", searchResult.Genres.ConvertAll(input => input.Name))
                             : string.Empty;

            Runtime.Text = searchResult.Runtime.ToString("g");

            if (AppSettings.MovieDBRatingSource == 0)
            {
                Rating.Text = searchResult.VoteAverage.ToString("g");
                Votes.Text = searchResult.VoteCount.ToString("g");
            }
            else
            {
                ImdbClient imdb = new ImdbClient();
                ImdbMovieData movieData = imdb.GetMovieById(searchResult.ImdbId);
                Rating.Text = movieData.Rating.ToString("g");
                Votes.Text = movieData.RatingCount.ToString("g");
            }

            Year.Text = searchResult.ReleaseDate.Year.ToString("g");
            Tagline.Text = searchResult.Tagline;
            Plot.Text = searchResult.Overview;

            if (movieKeywords != null && movieKeywords.Keywords != null)
                Keywords.Text = string.Join(", ", movieKeywords.Keywords.ConvertAll(input => input.Name));
            else
                Keywords.Text = string.Empty;

            ImdbId.Text = searchResult.ImdbId;

            Country.Text = searchResult.ProductionCountries != null
                               ? string.Join(" / ", searchResult.ProductionCountries.ConvertAll(input => input.Name))
                               : string.Empty;

            if (movieCasts != null && movieCasts.Crew != null)
            {
                Director.Text = string.Join(" / ",
                                            movieCasts.Crew.Where(crew => crew.Job == "Director")
                                                      .ToList()
                                                      .ConvertAll(input => input.Name));
                Writers.Text = string.Join(" / ",
                                           movieCasts.Crew.Where(crew => crew.Job == "Writer" || crew.Job == "Screenplay")
                                                     .ToList()
                                                     .ConvertAll(input => input.Name));
            }
            else
            {
                Director.Text = string.Empty;
                Writers.Text = string.Empty;
            }

            Studio.Text = searchResult.ProductionCompanies != null
                              ? string.Join(" / ", searchResult.ProductionCompanies.ConvertAll(input => input.Name))
                              : string.Empty;

            SetName.Text = searchResult.BelongsToCollection != null
                               ? string.Join(" / ", searchResult.BelongsToCollection.ConvertAll(input => input.Name))
                               : string.Empty;

            if (movieTrailers != null && movieTrailers.Youtube != null && movieTrailers.Youtube.Count > 0)
                Trailer.Text = "plugin://plugin.video.youtube/?action=play_video&amp;videoid=" +
                               movieTrailers.Youtube.First().Source;
            else
                Trailer.Text = string.Empty;

            Country selCountry =
                movieReleases.Countries.Single(country => country.Iso_3166_1.ToLowerInvariant() == selectedCertCountry) ??
                movieReleases.Countries.Single(country => country.Iso_3166_1.ToLowerInvariant() == fallbackCertCountry);

            MovieDBCertCountry certCountry =
                MovieDBCertCountries.CountryList.Single(country => country.CountryName == selectedCertCountry);

            MPAARating.Text = certCountry.Prefix + selCountry.Certification;


            // loading image sizes
            string posterOriginal = client.Config.Images.PosterSizes.Last();

            string posterPreview = client.Config.Images.PosterSizes.Count >= 2
                                       ? client.Config.Images.PosterSizes[client.Config.Images.PosterSizes.Count - 2]
                                       : client.Config.Images.PosterSizes.Last();

            string backdropOriginal = client.Config.Images.BackdropSizes.Last();

            string backdropPreview = client.Config.Images.BackdropSizes.Count >= 3
                                         ? client.Config.Images.BackdropSizes[
                                             client.Config.Images.BackdropSizes.Count - 3]
                                         : client.Config.Images.BackdropSizes.Last();
            
            // remove duplicate entries
            imageList.Backdrops.RemoveAt(imageList.Backdrops.FindIndex(data => data.FilePath == searchResult.BackdropPath));
            imageList.Posters.RemoveAt(imageList.Posters.FindIndex(data => data.FilePath == searchResult.PosterPath));


            // create image lists
            _postersList.Add(new MovieDBPosterImage
                {
                    Title = "Default",
                    UrlOriginal = client.GetImageUrl(posterOriginal, searchResult.PosterPath).AbsoluteUri,
                    UrlPreview = client.GetImageUrl(posterPreview, searchResult.PosterPath).AbsoluteUri
                });
            _backdropsList.Add(new MovieDBImageInfo
                {
                    Title = "Default",
                    UrlOriginal = client.GetImageUrl(backdropOriginal, searchResult.BackdropPath).AbsoluteUri,
                    UrlPreview = client.GetImageUrl(backdropPreview, searchResult.BackdropPath).AbsoluteUri
                });

            int cnt = 1;
            foreach (ImageData poster in imageList.Posters)
            {
                _postersList.Add(new MovieDBPosterImage
                    {
                        Title = "Online image " + cnt,
                        UrlOriginal = client.GetImageUrl(posterOriginal, poster.FilePath).AbsoluteUri,
                        UrlPreview = client.GetImageUrl(posterPreview, poster.FilePath).AbsoluteUri
                    });
                cnt++;
            }
            PosterList.ItemsSource = _postersList;
            PosterList.SelectedIndex = 0;

            cnt = 1;
            foreach (ImageData backdrop in imageList.Backdrops)
            {
                _backdropsList.Add(new MovieDBImageInfo
                {
                    Title = "Online image " + cnt,
                    UrlOriginal = client.GetImageUrl(backdropOriginal, backdrop.FilePath).AbsoluteUri,
                    UrlPreview = client.GetImageUrl(backdropPreview, backdrop.FilePath).AbsoluteUri
                });
                cnt++;
            }
            BackdropList.ItemsSource = _backdropsList;
            BackdropList.SelectedIndex = 0;

            

            foreach (Cast cast in movieCasts.Cast)
            {
                _castList.Casts.Add(new MovieDBCast
                    {
                        Name = cast.Name,
                        Role = cast.Character,
                        Thumbnail = client.GetImageUrl("original", cast.ProfilePath).AbsoluteUri
                    });
            }
            CastListView.ItemsSource = _castList.Casts;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ResultMovieData = new MovieEntry
                {
                    Casts = _castList.Casts,
                    Aired = "1969-12-31",
                    Premiered = "1969-12-31",
                    Code = "",
                    Countries = Country.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Directors = Director.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    EpBookmark = 0f,
                    FanartImages = _backdropsList,
                    Genres = Genre.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    ImdbID = ImdbId.Text,
                    LastPlayed = "1969-12-31",
                    MPAARating = MPAARating.Text,
                    OriginalTitle = MovieOriginalTitle.Text,
                    Outline = "",
                    PlayCount = 0,
                    Plot = Plot.Text,
                    PosterImages = _postersList,
                    Rating = float.Parse(Rating.Text),
                    Runtime = int.Parse(Runtime.Text),
                    SetNames = SetName.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    SortTitle = SortTitle.Text,
                    Status = "",
                    Studios = Studio.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Tagline = Tagline.Text,
                    Title = MovieTitle.Text,
                    Top250 = 0,
                    Trailer = Trailer.Text,
                    Votes = int.Parse(Votes.Text),
                    Writers = Writers.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Year = int.Parse(Year.Text)
                };
            ResultBackdropImage = ((MovieDBImageInfo)BackdropList.SelectedItem).UrlOriginal;
            ResultPosterImage = ((MovieDBPosterImage)PosterList.SelectedItem).UrlOriginal;

            DialogResult = true;
        }

        public void Dispose()
        {
        }
    }
}
